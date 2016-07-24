// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the COMMONCONTROLSACCESS_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// COMMONCONTROLSACCESS_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef COMMONCONTROLSACCESS_EXPORTS
#define COMMONCONTROLSACCESS_API __declspec(dllexport)
#else
#define COMMONCONTROLSACCESS_API __declspec(dllimport)
#endif

#define CCAMSG_LV_GETITEMTEXT (WM_APP+0)
#define CCAMSG_LV_GETITEMRECT (WM_APP+1)
#define CCAMSG_LV_GETSUBITEMRECT (WM_APP+2)

COMMONCONTROLSACCESS_API bool SetCommonControlsAccessHook(DWORD dwThreadId);

#define CCA_SHARED_MEMORY_NAME TEXT("\\\\.\\pipe\\CCAData")
#define CCA_SHARED_MEMORY_SIZE 4096
