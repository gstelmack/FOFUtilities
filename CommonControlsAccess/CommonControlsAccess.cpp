// CommonControlsAccess.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include "Resource.h"
#include "CommonControlsAccess.h"


#ifdef _MANAGED
#pragma managed(push, off)
#endif

// Forward references
LRESULT WINAPI GetMsgProc(int nCode, WPARAM wParam, LPARAM lParam);

INT_PTR WINAPI Dlg_Proc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);

#define StelmackSoftHANDLE_DLGMSG(hwnd, message, fn)       \
   case (message): return (SetDlgMsgResult(hwnd, uMsg,     \
   HANDLE_##message((hwnd), (wParam), (lParam), (fn))))

// This macro evaluates to the number of elements in an array. 
#define ARRAY_COUNT(Array) (sizeof(Array) / sizeof(Array[0]))

#ifdef _DEBUG
// This function forces the debugger to be invoked
void ForceDebugBreak() {
	__try { DebugBreak(); }
	__except(UnhandledExceptionFilter(GetExceptionInformation())) { }
}
#else
#define ForceDebugBreak()
#endif

///////////////////////////////////////////////////////////////////////////////


// Instruct the compiler to put the g_hhook data variable in 
// its own data section called Shared. We then instruct the 
// linker that we want to share the data in this section 
// with all instances of this application.
#pragma data_seg("Shared")
HHOOK g_hhook = NULL;
DWORD g_dwThreadIdCommonControlsAccess = 0;
HANDLE g_hSharedMemory = NULL;
#pragma data_seg()

// Instruct the linker to make the Shared section
// readable, writable, and shared.
#pragma comment(linker, "/section:Shared,rws")


///////////////////////////////////////////////////////////////////////////////


// Nonshared variables
HINSTANCE g_hinstDll = NULL;


///////////////////////////////////////////////////////////////////////////////


BOOL WINAPI DllMain(HINSTANCE hinstDll, DWORD fdwReason, PVOID fImpLoad) 
{
	switch (fdwReason) 
	{
	case DLL_PROCESS_ATTACH:
		// DLL is attaching to the address space of the current process.
		g_hinstDll = hinstDll;
		break;

	case DLL_THREAD_ATTACH:
		// A new thread is being created in the current process.
		break;

	case DLL_THREAD_DETACH:
		// A thread is exiting cleanly.
		break;

	case DLL_PROCESS_DETACH:
		// The calling process is detaching the DLL from its address space.
		break;
	}
	return(TRUE);
}


///////////////////////////////////////////////////////////////////////////////


bool SetCommonControlsAccessHook(DWORD dwThreadId) 
{
	bool fOk = false;

	if (dwThreadId != 0) 
	{
		// Make sure that the hook is not already installed.
		_ASSERT(g_hhook == NULL);

		// Save our thread ID in a shared variable so that our GetMsgProc 
		// function can post a message back to to thread when the server 
		// window has been created.
		g_dwThreadIdCommonControlsAccess = GetCurrentThreadId();

		// Install the hook on the specified thread
		g_hhook = SetWindowsHookEx(WH_GETMESSAGE, GetMsgProc, g_hinstDll, dwThreadId);

		fOk = (g_hhook != NULL);
		if (fOk) 
		{
			// The hook was installed successfully; force a benign message to 
			// the thread's queue so that the hook function gets called.
			fOk = (PostThreadMessage(dwThreadId, WM_NULL, 0, 0) == TRUE);
		}
	}
	else
	{
		// Make sure that a hook has been installed.
		_ASSERT(g_hhook != NULL);
		fOk = (UnhookWindowsHookEx(g_hhook) == TRUE);
		g_hhook = NULL;
	}

	return(fOk);
}


///////////////////////////////////////////////////////////////////////////////

void DisplayLastError(LPTSTR lpszMessage) 
{ 
    LPVOID lpMsgBuf;
	LPVOID lpDisplayBuf;
    DWORD dw = GetLastError(); 

    FormatMessage(
        FORMAT_MESSAGE_ALLOCATE_BUFFER | 
        FORMAT_MESSAGE_FROM_SYSTEM,
        NULL,
        dw,
        MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
        (LPTSTR) &lpMsgBuf,
        0, NULL );

    lpDisplayBuf = (LPVOID)LocalAlloc(LMEM_ZEROINIT, 
        (lstrlen((LPCTSTR)lpMsgBuf)+lstrlen((LPCTSTR)lpszMessage)+40)*sizeof(TCHAR)); 
    StringCchPrintf((LPTSTR)lpDisplayBuf, 
        LocalSize(lpDisplayBuf),
		TEXT("%s: %d %s"), 
        lpszMessage, dw, lpMsgBuf); 
    MessageBox(NULL, (LPCTSTR)lpDisplayBuf, TEXT("Error"), MB_OK); 

    LocalFree(lpMsgBuf);
    LocalFree(lpDisplayBuf);
}

LRESULT WINAPI GetMsgProc(int nCode, WPARAM wParam, LPARAM lParam) 
{
	static BOOL fFirstTime = TRUE;

	if (fFirstTime) 
	{
		// The DLL just got injected.
		fFirstTime = FALSE;

		//Connect to the server pipe using CreateFile()
		g_hSharedMemory = CreateFile( 
			CCA_SHARED_MEMORY_NAME,   // pipe name 
			GENERIC_WRITE,	// write access
			0,              // no sharing 
			NULL,           // default security attributes
			OPEN_EXISTING,  // opens existing pipe 
			0,              // default attributes 
			NULL);          // no template file

		if (g_hSharedMemory == INVALID_HANDLE_VALUE)
		{
			DisplayLastError("Could not open shared memory");
		}
		else
		{
			DWORD dwMode = PIPE_READMODE_MESSAGE; 
			BOOL bResult = SetNamedPipeHandleState( 
				g_hSharedMemory,    // pipe handle 
				&dwMode,  // new pipe mode 
				NULL,     // don't set maximum bytes 
				NULL);    // don't set maximum time 

			if (bResult)
			{
				// Create the CCA Server window to handle the client request.
				CreateDialog(g_hinstDll, MAKEINTRESOURCE(IDD_COMMON_CONTROLS_ACCESS), NULL, Dlg_Proc);
			}
			else
			{
				DisplayLastError("Could not set pipe to messaging");
				CloseHandle(g_hSharedMemory);
			}
		}

		// Tell the application that the server is up 
		// and ready to handle requests.
		PostThreadMessage(g_dwThreadIdCommonControlsAccess, WM_NULL, 0, 0);
	}

	return(CallNextHookEx(g_hhook, nCode, wParam, lParam));
}

void Dlg_OnClose(HWND hwnd) 
{
	if (g_hSharedMemory)
	{
		CloseHandle(g_hSharedMemory);
		g_hSharedMemory = NULL;
	}

	DestroyWindow(hwnd);
}

INT_PTR WINAPI Dlg_Proc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam) 
{
	switch (uMsg) 
	{
		StelmackSoftHANDLE_DLGMSG(hwnd, WM_CLOSE, Dlg_OnClose);

	case CCAMSG_LV_GETITEMTEXT:
		{
			DWORD bytesWritten;
			TCHAR szName[MAX_PATH];
			int itemIndex = LOWORD(lParam);
			int subItemIndex = HIWORD(lParam);
			ListView_GetItemText((HWND)wParam, itemIndex, subItemIndex, szName, ARRAY_COUNT(szName));
			if (g_hSharedMemory)
			{
				BOOL bResult = WriteFile( 
					g_hSharedMemory,	// handle to pipe 
					szName,				// buffer to write from 
					(DWORD)((_tcslen(szName)+1)*sizeof(TCHAR)), // number of bytes to write, include the NULL 
					&bytesWritten,      // number of bytes written 
					NULL);              // not overlapped I/O 
				if (!bResult)
				{
					DisplayLastError("Could not write to shared memory");
				}
			}
			break;
		}
	case CCAMSG_LV_GETITEMRECT:
		{
			DWORD bytesWritten;
			TCHAR szRect[MAX_PATH];
			int itemIndex = LOWORD(lParam);
			RECT rect;
			ListView_GetItemRect((HWND)wParam, itemIndex, &rect, LVIR_BOUNDS);
			if (g_hSharedMemory)
			{
				StringCchPrintf(szRect,MAX_PATH,"%d,%d,%d,%d",rect.left,rect.top,rect.right,rect.bottom);
				BOOL bResult = WriteFile( 
					g_hSharedMemory,	// handle to pipe 
					szRect,				// buffer to write from 
					(DWORD)((_tcslen(szRect)+1)*sizeof(TCHAR)), // number of bytes to write, include the NULL 
					&bytesWritten,      // number of bytes written 
					NULL);              // not overlapped I/O 
				if (!bResult)
				{
					DisplayLastError("Could not write to shared memory");
				}
			}
			break;
		}
	case CCAMSG_LV_GETSUBITEMRECT:
		{
			DWORD bytesWritten;
			TCHAR szRect[MAX_PATH];
			int itemIndex = LOWORD(lParam);
			int subItemIndex = HIWORD(lParam);
			RECT rect;
			ListView_GetSubItemRect((HWND)wParam, itemIndex, subItemIndex, LVIR_BOUNDS, &rect);
			if (g_hSharedMemory)
			{
				StringCchPrintf(szRect,MAX_PATH,"%d,%d,%d,%d",rect.left,rect.top,rect.right,rect.bottom);
				BOOL bResult = WriteFile( 
					g_hSharedMemory,	// handle to pipe 
					szRect,				// buffer to write from 
					(DWORD)((_tcslen(szRect)+1)*sizeof(TCHAR)), // number of bytes to write, include the NULL 
					&bytesWritten,      // number of bytes written 
					NULL);              // not overlapped I/O 
				if (!bResult)
				{
					DisplayLastError("Could not write to shared memory");
				}
			}
			break;
		}
	}

	return(FALSE);
}
