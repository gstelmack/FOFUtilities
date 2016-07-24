
// DraftRecorderDlg.cpp : implementation file
//

#include "stdafx.h"
#include "DraftRecorder.h"
#include "DraftRecorderDlg.h"
#include "VersionNumber.h"

#include "CommonControlsAccess.h"

#include <vector>

#include <strsafe.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

void DisplayError(LPCTSTR lpszMessage)
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
	int result = AfxMessageBox((LPTSTR)lpDisplayBuf,MB_RETRYCANCEL|MB_ICONSTOP);
    LocalFree(lpMsgBuf);
    LocalFree(lpDisplayBuf);
	if (result == IDCANCEL)
	{
		AfxAbort();
	}
}

// CAboutDlg dialog used for App About

class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// Dialog Data
	enum { IDD = IDD_ABOUTBOX };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Implementation
protected:
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
END_MESSAGE_MAP()


// CDraftRecorderDlg dialog




CDraftRecorderDlg::CDraftRecorderDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CDraftRecorderDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CDraftRecorderDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_EDIT_PLAYERS, m_playersEdit);
}

BEGIN_MESSAGE_MAP(CDraftRecorderDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	//}}AFX_MSG_MAP
	ON_BN_CLICKED(IDC_BUTTON_DRAFT_PLAYERS, &CDraftRecorderDlg::OnBnClickedButtonDraftPlayers)
END_MESSAGE_MAP()


// CDraftRecorderDlg message handlers

BOOL CDraftRecorderDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Add "About..." menu item to system menu.

	// IDM_ABOUTBOX must be in the system command range.
	ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
	ASSERT(IDM_ABOUTBOX < 0xF000);

	CMenu* pSysMenu = GetSystemMenu(FALSE);
	if (pSysMenu != NULL)
	{
		BOOL bNameValid;
		CString strAboutMenu;
		bNameValid = strAboutMenu.LoadString(IDS_ABOUTBOX);
		ASSERT(bNameValid);
		if (!strAboutMenu.IsEmpty())
		{
			pSysMenu->AppendMenu(MF_SEPARATOR);
			pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
		}
	}

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	CString windowTitle;
	GetWindowText(windowTitle);
	windowTitle += " v";
	windowTitle += STRFILEVER;
	SetWindowText(windowTitle);

	return TRUE;  // return TRUE  unless you set the focus to a control
}

void CDraftRecorderDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CDialog::OnSysCommand(nID, lParam);
	}
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CDraftRecorderDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}

// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CDraftRecorderDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}

HANDLE g_hSharedMemory = NULL;
LPTSTR g_pSharedMemory = NULL;
bool OpenSharedMemory()
{
	PSECURITY_DESCRIPTOR pSD = NULL; 
	pSD = HeapAlloc(GetProcessHeap(), 0, SECURITY_DESCRIPTOR_MIN_LENGTH);
	if(pSD == NULL)
	{
		DisplayError(TEXT("Could not allocate security descriptor"));
		return false;
	}

	// We now have an empty security descriptor
	if (!InitializeSecurityDescriptor(pSD,
		SECURITY_DESCRIPTOR_REVISION))
	{
		DisplayError(TEXT("Could not initialize security descriptor"));
		HeapFree(GetProcessHeap(), 0, pSD);
		return false;
	}

	if(!SetSecurityDescriptorDacl(pSD, TRUE, NULL, FALSE))
	{
		DisplayError(TEXT("Could not set security descriptor DACL"));
		HeapFree(GetProcessHeap(), 0, pSD);
		return false;
	}

	// Then we point to our SD from a SECURITY_ATTRIBUTES structure
	SECURITY_ATTRIBUTES sa = {0};
	sa.nLength = sizeof(sa);
	sa.lpSecurityDescriptor = pSD;

	g_hSharedMemory = CreateNamedPipe(CCA_SHARED_MEMORY_NAME,
		PIPE_ACCESS_INBOUND,      // read access 
		PIPE_TYPE_MESSAGE |       // message type pipe 
		PIPE_READMODE_MESSAGE |   // message-read mode 
		PIPE_NOWAIT,	          // non-blocking mode 
		PIPE_UNLIMITED_INSTANCES, // max. instances  
		CCA_SHARED_MEMORY_SIZE,   // output buffer size 
		CCA_SHARED_MEMORY_SIZE,   // input buffer size 
		2000,						// client time-out 
		&sa);                    // default security attribute 

	if (g_hSharedMemory == NULL) 
	{
		DisplayError(TEXT("Could not create shared memory"));
		HeapFree(GetProcessHeap(), 0, pSD);
		return false;
	}

	return true;
}

void CloseSharedMemory()
{
	if (g_hSharedMemory)
	{
		CloseHandle(g_hSharedMemory);
		g_hSharedMemory = NULL;
	}
}

HWND g_hwndCCA = NULL;
bool OpenCCAHook(HWND parentWnd)
{
	if (!OpenSharedMemory())
	{
		return false;
	}

	if (!SetCommonControlsAccessHook(GetWindowThreadProcessId(parentWnd, NULL)))
	{
		DisplayError(TEXT("Could not create hook"));
		return false;
	}

	Sleep(500);

	// Wait for the server window to be created.
	MSG msg;
	GetMessage(&msg,(HWND)-1,0,0);

	// Find the handle of the hidden dialog box window.
	g_hwndCCA = ::FindWindow(NULL, TEXT("StelmackSoftCCA"));
	if (!IsWindow(g_hwndCCA))
	{
		CloseSharedMemory();
		DisplayError(TEXT("Could not connect to CCA lib"));
		return false;
	}
	
	return true;
}

void CloseCCAHook()
{
	if (g_hwndCCA)
	{
		// Tell the CCA window to destroy itself. Use SendMessage 
		// instead of PostMessage so that we know the window is 
		// destroyed before the hook is removed.
		::SendMessage(g_hwndCCA, WM_CLOSE, 0, 0);

		// Make sure that the window was destroyed.
		ASSERT(!IsWindow(g_hwndCCA));

		// Unhook the DLL, removing the dialog box procedure 
		// from the FOF's address space.
		SetCommonControlsAccessHook(0);  
	}

	g_hwndCCA = NULL;

	CloseSharedMemory();
}

CString ReadSharedMemory()
{
	CString retVal;
	TCHAR szBuffer[CCA_SHARED_MEMORY_SIZE];
	DWORD readBytes;

	//Read client message
	BOOL bResult = ReadFile( 
		g_hSharedMemory,	// handle to pipe 
		szBuffer,			// buffer to receive data 
		sizeof(szBuffer),	// size of buffer 
		&readBytes,			// number of bytes read 
		NULL);				// not overlapped I/O 

	if (bResult)
	{
		retVal = szBuffer;
		// Check for special codes at the front of strings and remove them
		if (retVal.GetLength() > 2)
		{
			if (retVal.GetAt(0) == '$' && retVal.GetAt(1) == '$')
			{
				retVal = retVal.Right(retVal.GetLength() - 2);
			}
			else if (retVal.GetAt(0) == '*' && retVal.GetAt(1) == '*')
			{
				retVal = retVal.Right(retVal.GetLength() - 2);
			}
		}
	}
	else
	{
		DisplayError(TEXT("Could not read from shared memory"));
	}

	return retVal;
}

struct FOFPlayerInfo
{
	CString Name;
	CString College;
	CString Position;
};

CString PositionGroups[] =
{
"NA"
,"QB"
,"RB"
,"FB"
,"TE"
,"WR"
,"C"
,"G"
,"T"
,"P"
,"K"
,"DE"
,"DT"
,"ILB"
,"OLB"
,"CB"
,"S"
};

void CDraftRecorderDlg::OnBnClickedButtonDraftPlayers()
{
	CString windowName;
	CWnd* draftWindow;
	for (draftWindow = GetWindow(GW_HWNDFIRST);draftWindow != NULL;draftWindow = draftWindow->GetWindow(GW_HWNDNEXT))
	{
		draftWindow->GetWindowText(windowName);
		if (windowName.Find(TEXT(" Amateur Draft, "))>= 0)
		{
			break;
		}
	}
	if (!draftWindow)
	{
		AfxMessageBox(TEXT("Cannot find Draft Class window.\n")
			,MB_OK|MB_ICONSTOP);
		return;
	}

	m_scoutingReport = (CListCtrl*)draftWindow->GetDlgItem(0x5a8);
	m_draftPlayerWindow = draftWindow->GetDlgItem(0x510);

	std::vector<FOFPlayerInfo> fofPlayers;

	if (!OpenCCAHook(draftWindow->GetSafeHwnd()))
	{
		return;
	}

	fofPlayers.resize(m_scoutingReport->GetItemCount());
	for (int itemIndex=0;itemIndex<m_scoutingReport->GetItemCount();++itemIndex)
	{
		LPARAM lParam = MAKELPARAM(itemIndex,0);
		::SendMessage(g_hwndCCA, CCAMSG_LV_GETITEMTEXT, (WPARAM)m_scoutingReport->m_hWnd, lParam);
		CString tempName = ReadSharedMemory();
		int splitPosition = tempName.Find(TEXT(", "));
		CString lastName = tempName.Left(splitPosition);
		CString firstName = tempName.Mid(splitPosition + 2);
		fofPlayers[itemIndex].Name = firstName + " " + lastName;

		lParam = MAKELPARAM(itemIndex,1);
		::SendMessage(g_hwndCCA, CCAMSG_LV_GETITEMTEXT, (WPARAM)m_scoutingReport->m_hWnd, lParam);
		fofPlayers[itemIndex].Position = PositionGroups[::atoi(ReadSharedMemory())+1];

		lParam = MAKELPARAM(itemIndex,13);
		::SendMessage(g_hwndCCA, CCAMSG_LV_GETITEMTEXT, (WPARAM)m_scoutingReport->m_hWnd, lParam);
		fofPlayers[itemIndex].College = ReadSharedMemory();
	}

	CloseCCAHook();

	int playerCount = m_playersEdit.GetLineCount();
	CString strText;
	for (int draftedPlayerIndex = 0;draftedPlayerIndex<playerCount;draftedPlayerIndex++)
	{
		// length of line i:
		int len = m_playersEdit.LineLength(m_playersEdit.LineIndex(draftedPlayerIndex));
		m_playersEdit.GetLine(draftedPlayerIndex, strText.GetBuffer(len), len);
		strText.ReleaseBuffer(len);

		int lNamePosition;
		//These two lines allow FOF draft results to be used.
		//lNamePosition = strText.Find(TEXT(" - ")) + 3;
		//if (lNamePosition < 3)
		{
			lNamePosition = strText.Find(TEXT(". ")) + 2;
			if (lNamePosition < 2)
			{
				continue;
			}
		}
		if (strText.GetAt(lNamePosition) == TEXT('-'))
		{
			lNamePosition += 2;
		}
		int fNamePosition = strText.Find(TEXT(", "),lNamePosition);
		CString lastName = strText.Mid(lNamePosition,fNamePosition - lNamePosition);
		fNamePosition += 2;
		int positionPosition = strText.Find(TEXT(", "),fNamePosition);
		CString firstName = strText.Mid(fNamePosition,positionPosition - fNamePosition);
		positionPosition += 2;
		int collegePosition = strText.Find(TEXT(", "),positionPosition);
		CString position = strText.Mid(positionPosition,collegePosition - positionPosition);
		collegePosition += 2;
		CString college = strText.Mid(collegePosition);
		CString playerName = firstName + " " + lastName;

		bool foundPlayer = false;
		for (int i=0;i<(int)fofPlayers.size();++i)
		{
			if (fofPlayers[i].Name == playerName && fofPlayers[i].College == college && fofPlayers[i].Position == position)
			{
				foundPlayer = DraftPlayer(i);
				if (!foundPlayer)
				{
					foundPlayer = DraftPlayer(i);
				}
				fofPlayers.erase(fofPlayers.begin() + i);
				break;
			}
		}

		if (!foundPlayer)
		{
			CString message;
			message.Format("Could not find player:\n\n%s\n\nIn draft list. Fix Analyzer file and/or manually draft player, then re-run.",strText);
			AfxMessageBox(message,MB_OK|MB_ICONSTOP);
			return;
		}
	}
}

bool CDraftRecorderDlg::DraftPlayer(int playerIndex)
{
	m_scoutingReport->EnsureVisible(0,FALSE);

	POINT clickPoint;
	// Should put us on the first item
	clickPoint.x = 10;
	clickPoint.y = 23;
	m_scoutingReport->ClientToScreen(&clickPoint);

	// Need screen coordinates to figure out the mapping
	int screenX = GetSystemMetrics(SM_CXSCREEN);
	int screenY = GetSystemMetrics(SM_CYSCREEN);
	// Convert to the 0-65K coordinates used by SendInput
	long clickX = (clickPoint.x*65535)/screenX;
	long clickY = (clickPoint.y*65535)/screenY;

	INPUT inputStruct;
	ZeroMemory(&inputStruct,sizeof(inputStruct));
	inputStruct.type = INPUT_MOUSE;

	// Set the click point on a row
	inputStruct.mi.dx = clickX;
	inputStruct.mi.dy = clickY;

	inputStruct.mi.dwFlags = MOUSEEVENTF_ABSOLUTE|MOUSEEVENTF_MOVE;
	SendInput(1,&inputStruct,sizeof(inputStruct));
	Sleep(50);
	inputStruct.mi.dwFlags = MOUSEEVENTF_ABSOLUTE|MOUSEEVENTF_LEFTDOWN;
	SendInput(1,&inputStruct,sizeof(inputStruct));
	Sleep(50);
	inputStruct.mi.dwFlags = MOUSEEVENTF_ABSOLUTE|MOUSEEVENTF_LEFTUP;
	SendInput(1,&inputStruct,sizeof(inputStruct));
	Sleep(50);

	for (int i=0;i<playerIndex;++i)
	{
		inputStruct.type = INPUT_KEYBOARD;
		inputStruct.ki.dwExtraInfo = 0;
		inputStruct.ki.dwFlags = 0;
		inputStruct.ki.time = 0;
		inputStruct.ki.wScan = 0;
		inputStruct.ki.wVk = VK_DOWN;
		SendInput(1,&inputStruct,sizeof(inputStruct));
		Sleep(50);

		//inputStruct.type = INPUT_KEYBOARD;
		//inputStruct.ki.dwExtraInfo = 0;
		//inputStruct.ki.dwFlags = KEYEVENTF_KEYUP;
		//inputStruct.ki.time = 0;
		//inputStruct.ki.wScan = 0;
		//inputStruct.ki.wVk = VK_DOWN;
		//SendInput(1,&inputStruct,sizeof(inputStruct));
		//Sleep(50);
	}

	if (m_scoutingReport->GetSelectionMark() == playerIndex)
	{
		::SendMessage(m_draftPlayerWindow->GetSafeHwnd(),BM_CLICK,0,0);
		return true;
	}
	else
	{
		AfxMessageBox("Currently selected row is not the expected row. May need to re-run, something got off. Utility will try one more time.",MB_OK|MB_ICONSTOP);
		return false;
	}
}