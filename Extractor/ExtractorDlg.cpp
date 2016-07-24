// ExtractorDlg.cpp : implementation file
//

#include "stdafx.h"
#include "Extractor.h"
#include "ExtractorDlg.h"
#include "VersionNumber.h"

#include <fstream>
#include <list>
#include <vector>
#include <iomanip>
#include <map>
#include <queue>

#include <shlobj.h>
#include <shlwapi.h>
#include <Sddl.h>
#include <strsafe.h>
#include <math.h>

#include "CommonControlsAccess.h"
#include "DataTable.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//#define DEBUG_FONT_OUTPUT
//#define DEBUG_STAFF_OCR

#ifdef DEBUG_STAFF_OCR
std::ofstream gDebugFile;
#endif

// CAboutDlg dialog used for App About

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

static const int kMaxAbility = 100;
static CString kAbilityMap[kMaxAbility+1] =
{
	"Poor"
	,"Poor"
	,"Poor"
	,"Poor"
	,"Poor"
	,"Poor"
	,"Poor"
	,"Poor"
	,"Poor"
	,"Poor"
	,"Poor"
	,"Poor"
	,"Poor"
	,"Poor"
	,"Poor"
	,"Poor"
	,"Fair"
	,"Fair"
	,"Fair"
	,"Fair"
	,"Fair"
	,"Fair"
	,"Fair"
	,"Fair"
	,"Fair"
	,"Fair"
	,"Fair"
	,"Fair"
	,"Fair"
	,"Fair"
	,"Fair"
	,"Average"
	,"Average"
	,"Average"
	,"Average"
	,"Average"
	,"Average"
	,"Average"
	,"Average"
	,"Average"
	,"Average"
	,"Average"
	,"Average"
	,"Average"
	,"Average"
	,"Average"
	,"Average"
	,"Average"
	,"Average"
	,"Average"
	,"Average"
	,"Good"
	,"Good"
	,"Good"
	,"Good"
	,"Good"
	,"Good"
	,"Good"
	,"Good"
	,"Good"
	,"Good"
	,"Good"
	,"Good"
	,"Good"
	,"Good"
	,"Good"
	,"Good"
	,"Good"
	,"Good"
	,"Good"
	,"Very Good"
	,"Very Good"
	,"Very Good"
	,"Very Good"
	,"Very Good"
	,"Very Good"
	,"Very Good"
	,"Very Good"
	,"Very Good"
	,"Very Good"
	,"Very Good"
	,"Very Good"
	,"Very Good"
	,"Very Good"
	,"Very Good"
	,"Excellent"
	,"Excellent"
	,"Excellent"
	,"Excellent"
	,"Excellent"
	,"Excellent"
	,"Excellent"
	,"Excellent"
	,"Excellent"
	,"Excellent"
	,"Excellent"
	,"Excellent"
	,"Excellent"
	,"Excellent"
	,"Excellent"
	,"Excellent"
};


// CExtractorDlg dialog

std::map<CString,CString> gPositionGroupMap;
std::map<CString,int> gPositionAttributeCountMap;
TCHAR gDestinationDirectory[MAX_PATH];
std::map<CString,CString> gTeamImageMap;

CExtractorDlg::CExtractorDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CExtractorDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

	gPositionGroupMap["QB"] = "QB";
	gPositionGroupMap["RB"] = "RB";
	gPositionGroupMap["FB"] = "FB";
	gPositionGroupMap["TE"] = "TE";
	gPositionGroupMap["SE"] = "WR";
	gPositionGroupMap["FL"] = "WR";
	gPositionGroupMap["C"] = "C";
	gPositionGroupMap["LG"] = "G";
	gPositionGroupMap["LT"] = "T";
	gPositionGroupMap["RG"] = "G";
	gPositionGroupMap["RT"] = "T";
	gPositionGroupMap["LDE"] = "DE";
	gPositionGroupMap["RDE"] = "DE";
	gPositionGroupMap["RDT"] = "DT";
	gPositionGroupMap["LDT"] = "DT";
	gPositionGroupMap["NT"] = "DT";
	gPositionGroupMap["SILB"] = "ILB";
	gPositionGroupMap["WILB"] = "ILB";
	gPositionGroupMap["WLB"] = "OLB";
	gPositionGroupMap["SLB"] = "OLB";
	gPositionGroupMap["MLB"] = "ILB";
	gPositionGroupMap["LCB"] = "CB";
	gPositionGroupMap["RCB"] = "CB";
	gPositionGroupMap["SS"] = "S";
	gPositionGroupMap["FS"] = "S";
	gPositionGroupMap["P"] = "P";
	gPositionGroupMap["K"] = "K";

	gPositionAttributeCountMap["QB"] = 13;
	gPositionAttributeCountMap["RB"] = 15;
	gPositionAttributeCountMap["FB"] = 12;
	gPositionAttributeCountMap["TE"] = 13;
	gPositionAttributeCountMap["SE"] = 11;
	gPositionAttributeCountMap["FL"] = 11;
	gPositionAttributeCountMap["C"] = 5;
	gPositionAttributeCountMap["LG"] = 4;
	gPositionAttributeCountMap["LT"] = 4;
	gPositionAttributeCountMap["RG"] = 4;
	gPositionAttributeCountMap["RT"] = 4;
	gPositionAttributeCountMap["LDE"] = 6;
	gPositionAttributeCountMap["RDE"] = 6;
	gPositionAttributeCountMap["RDT"] = 6;
	gPositionAttributeCountMap["LDT"] = 6;
	gPositionAttributeCountMap["NT"] = 6;
	gPositionAttributeCountMap["SILB"] = 10;
	gPositionAttributeCountMap["WILB"] = 10;
	gPositionAttributeCountMap["WLB"] = 10;
	gPositionAttributeCountMap["SLB"] = 10;
	gPositionAttributeCountMap["MLB"] = 10;
	gPositionAttributeCountMap["LCB"] = 11;
	gPositionAttributeCountMap["RCB"] = 11;
	gPositionAttributeCountMap["SS"] = 11;
	gPositionAttributeCountMap["FS"] = 11;
	gPositionAttributeCountMap["P"] = 4;
	gPositionAttributeCountMap["K"] = 4;

	if(SUCCEEDED(SHGetFolderPath(NULL, 
								 CSIDL_PERSONAL|CSIDL_FLAG_CREATE, 
								 NULL, 
								 0, 
								 gDestinationDirectory))) 
	{
		PathAppend(gDestinationDirectory,"StelmackSoft");
		CreateDirectory(gDestinationDirectory,NULL);
		PathAppend(gDestinationDirectory,"UtilitySuite");
		CreateDirectory(gDestinationDirectory,NULL);
	}
}

CExtractorDlg::~CExtractorDlg()
{
#ifdef DEBUG_STAFF_OCR
	gDebugFile.close();
#endif
}

void CExtractorDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_CURRENT_PLAYER_STATIC, mCurrentPlayerStatic);
	DDX_Control(pDX, IDC_CHECK_PROCESS_FREE_AGENTS, mProcessFreeAgentsCheck);
    DDX_Control(pDX, IDC_BUTTON_FILENAME, mOutputFilenameButton);
}

BEGIN_MESSAGE_MAP(CExtractorDlg, CDialog)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	//}}AFX_MSG_MAP
	ON_BN_CLICKED(IDC_EXTRACT_BUTTON, OnBnClickedExtractButton)
    ON_BN_CLICKED(IDC_BUTTON_FILENAME, &CExtractorDlg::OnBnClickedButtonFilename)
END_MESSAGE_MAP()


// CExtractorDlg message handlers

BOOL CExtractorDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

#ifdef DEBUG_STAFF_OCR
	gDebugFile.open("staff_debug.txt");
#endif

	mCurrentPlayerStatic.SetWindowText("");

	CString windowTitle;
	GetWindowText(windowTitle);
	windowTitle += " v";
	windowTitle += STRFILEVER;
	SetWindowText(windowTitle);

	TCHAR srcPath[MAX_PATH];
	strcpy_s(srcPath,gDestinationDirectory);
	PathAppend(srcPath,"TeamImageMap.csv");
	DataTable teamMap;
	if (teamMap.LoadCSV(srcPath))
	{
		for (size_t rowIndex=0;rowIndex<teamMap.GetRowCount();++rowIndex)
		{
			if (teamMap.GetColumnCount(rowIndex)>=2)
			{
				CString team = teamMap.GetCell(rowIndex,0);
				CString imagePath = teamMap.GetCell(rowIndex,1);
				gTeamImageMap[team] = imagePath;
			}
		}
	}

    TCHAR destPath[MAX_PATH];
    strcpy_s(destPath,gDestinationDirectory);
    PathAppend(destPath,"FOFRoster.csv");
    mOutputFilenameButton.SetWindowText(destPath);

	return TRUE;  // return TRUE  unless you set the focus to a control
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CExtractorDlg::OnPaint()
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
HCURSOR CExtractorDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}

struct PlayerData
{
	PlayerData() :
			mName("")
			,mPosition("")
			,mPositionGroup("")
			,mTeam("")
			,mBirthDate("")
			,mAgent("")
			,mHometown("")
			,mCollege("")
			,mDesignation("")
			,mWeight(0)
			,mHeight(0)
			,mJerseyNumber(0)
			,mExperience(0)
			,mVolatility(0)
			,mSolecismicTest(0)
			,m40YardTimeWhole(0)
			,m40YardTimeFraction(0)
			,mBenchPressReps(0)
			,mAgilityWhole(0)
			,mAgilityFraction(0)
			,mBroadJumpFeet(0)
			,mBroadJumpInches(0)
			,mPositionDrill(0)
			,mPercentDeveloped(0)
			,mInterviewed("")
			,mImpression("")
			,mConflicts("")
			,mAffinities("")
			,mCharacter("")
			,mLoyalty(0)
			,mWantsWinner(0)
			,mLeadership(0)
			,mIntelligence(0)
			,mPersonality(0)
			,mPopularity(0)
			,mMentor("")
			,mFormations(0)
			,mCurrent(0)
			,mFuture(0)
		{
		}

	CString mName;
	CString mPosition;
	CString mPositionGroup;
	CString mTeam;
	CString mBirthDate;
	CString mAgent;
	CString mHometown;
	CString mCollege;
	CString mDesignation;
	USHORT	mWeight;
	BYTE	mHeight;
	BYTE	mJerseyNumber;
	BYTE	mExperience;
	BYTE	mVolatility;
	BYTE	mSolecismicTest;
	BYTE	m40YardTimeWhole;
	BYTE	m40YardTimeFraction;
	BYTE	mBenchPressReps;
	BYTE	mAgilityWhole;
	BYTE	mAgilityFraction;
	BYTE	mBroadJumpFeet;
	BYTE	mBroadJumpInches;
	BYTE	mPositionDrill;
	BYTE	mPercentDeveloped;
	BYTE	mLoyalty;
	BYTE	mWantsWinner;
	BYTE	mLeadership;
	BYTE	mIntelligence;
	BYTE	mPersonality;
	BYTE	mPopularity;
	CString	mMentor;
	CString	mInterviewed;
	CString mImpression;
	CString mConflicts;
	CString mAffinities;
	CString mCharacter;
	BYTE	mCurrent;
	BYTE	mFuture;
	BYTE	mFormations;
	BYTE	mLowStats[20];
	BYTE	mHighStats[20];
};

static const int kScoutReputationCount = 9;
struct ScoutData
{
	CString mName;
	CString mCurrentTeam;
	UINT	mAskingPrice;
	BYTE	mContractYears;
	BYTE	mAge;
	BYTE	mPlayoffs;
	BYTE	mBowlWins;
	CString mReputationData[kScoutReputationCount];
};

static const int kCoachReputationCount = 14;
struct CoachData
{
	CString mName;
	CString mPosition;
	CString mCurrentTeam;
	UINT	mAskingPrice;
	BYTE	mContractYears;
	BYTE	mAge;
	BYTE	mExperience;
	BYTE	mPlayoffs;
	BYTE	mBowlWins;
	CString mReputationData[kCoachReputationCount];
};

bool	gHaveDraftData;

int GetDetailedNumber(const CString& value)
{
	int detail = 0;
	int startIndex = value.Find(_T('('));
	if (startIndex >= 0)
	{
		int endIndex = value.Find(_T(')'),startIndex+1);
		if (endIndex > startIndex)
		{
			CString tmpString = value.Mid(startIndex+1,endIndex-startIndex-1);
			detail = atoi(tmpString);
		}
	}

	return detail;
}

void ProcessLine(const CString& label, const CString& value, ScoutData& curScout)
{
	int years;
	int count;
	if (label == "Current Team")
	{
		curScout.mCurrentTeam = value;
	}
	else if (label == "Contract Years")
	{
		years = atoi(value);
		curScout.mContractYears = (BYTE)(years);
	}
	else if (label == "Asking Price"
		|| label == "Salary")
	{
		UINT dollars = 0;
		for (int charIndex=0;charIndex<value.GetLength();charIndex++)
		{
			TCHAR curChar = value.GetAt(charIndex);
			if (_istdigit(curChar))
			{
				dollars *= 10;
				dollars += (curChar - _T('0'));
			}
		}
		curScout.mAskingPrice = dollars;
	}
	else if (label == "Age")
	{
		years = atoi(value);
		curScout.mAge = (BYTE)(years);
	}
	else if (label == "Playoffs")
	{
		count = atoi(value);
		curScout.mPlayoffs = (BYTE)(count);
	}
	else if (label == "Bowl Wins")
	{
		count = atoi(value);
		curScout.mBowlWins = (BYTE)(count);
	}
}

void ProcessLine(const CString& label, const CString& value, CoachData& curCoach)
{
	int years;
	int count;
	if (label == "Current Role")
	{
		curCoach.mPosition = value;
	}
	else if (label == "Current Team")
	{
		curCoach.mCurrentTeam = value;
	}
	else if (label == "Contract Years")
	{
		years = atoi(value);
		curCoach.mContractYears = (BYTE)(years);
	}
	else if (label == "Asking Price"
		|| label == "Salary")
	{
		UINT dollars = 0;
		for (int charIndex=0;charIndex<value.GetLength();charIndex++)
		{
			TCHAR curChar = value.GetAt(charIndex);
			if (_istdigit(curChar))
			{
				dollars *= 10;
				dollars += (curChar - _T('0'));
			}
		}
		curCoach.mAskingPrice = dollars;
	}
	else if (label == "Age")
	{
		years = atoi(value);
		curCoach.mAge = (BYTE)(years);
	}
	else if (label == "Playoffs")
	{
		count = atoi(value);
		curCoach.mPlayoffs = (BYTE)(count);
	}
	else if (label == "Bowl Wins")
	{
		count = atoi(value);
		curCoach.mBowlWins = (BYTE)(count);
	}
	else if (label == "Experience")
	{
		years = atoi(value);
		curCoach.mExperience = (BYTE)(years);
	}
}

void ProcessLine(const CString& label, const CString& value, PlayerData& curPlayer)
{
	if (label == "Height")
	{
		int feet;
		int inches;
		sscanf_s(value,"%d-%d",&feet,&inches);
		curPlayer.mHeight = (BYTE)((feet*12)+inches);
	}
	else if (label == "Weight")
	{
		int weight = atoi(value);
		curPlayer.mWeight = (USHORT)(weight);
	}
	else if (label == "College")
	{
		curPlayer.mCollege = value;
	}
	else if (label == "Team")
	{
		curPlayer.mTeam = value;
	}
	else if (label == "Born")
	{
		curPlayer.mBirthDate = value;
	}
	else if (label == "Home Town")
	{
		curPlayer.mHometown = value;
	}
	else if (label == "Agent")
	{
		curPlayer.mAgent = value;
	}
	else if (label == "Designation")
	{
		curPlayer.mDesignation = value;
	}
	else if (label == "Volatility")
	{
		curPlayer.mVolatility = (BYTE)(GetDetailedNumber(value));
	}
	else if (label == "Solecismic Test")
	{
		curPlayer.mSolecismicTest = (BYTE)(GetDetailedNumber(value));
		gHaveDraftData = true;
	}
	else if (label == "40-Yard Dash")
	{
		if (value == "No Workout")
		{
			curPlayer.m40YardTimeWhole = 0;
			curPlayer.m40YardTimeFraction = 0;
		}
		else
		{
			int dashWhole;
			int dashFraction;
			sscanf_s(value,"%d.%d",&dashWhole,&dashFraction);
			curPlayer.m40YardTimeWhole = (BYTE)(dashWhole);
			curPlayer.m40YardTimeFraction = (BYTE)(dashFraction);
		}
	}
	else if (label == "Bench Press")
	{
		if (value == "No Workout")
		{
			curPlayer.mBenchPressReps = 0;
		}
		else
		{
			int benchPress;
			sscanf_s(value,"%d",&benchPress);
			curPlayer.mBenchPressReps = (BYTE)(benchPress);
		}
	}
	else if (label == "Agility Drill")
	{
		if (value == "No Workout")
		{
			curPlayer.mAgilityWhole = 0;
			curPlayer.mAgilityFraction = 0;
		}
		else
		{
			int agilityWhole;
			int agilityFraction;
			sscanf_s(value,"%d.%d",&agilityWhole,&agilityFraction);
			curPlayer.mAgilityWhole = (BYTE)(agilityWhole);
			curPlayer.mAgilityFraction = (BYTE)(agilityFraction);
		}
	}
	else if (label == "Broad Jump")
	{
		if (value == "No Workout")
		{
			curPlayer.mBroadJumpFeet = 0;
			curPlayer.mBroadJumpInches = 0;
		}
		else
		{
			int broadJumpFeet;
			int broadJumpInches;
			sscanf_s(value,"%d ft., %d in.",&broadJumpFeet,&broadJumpInches);
			curPlayer.mBroadJumpFeet = (BYTE)broadJumpFeet;
			curPlayer.mBroadJumpInches = (BYTE)broadJumpInches;
		}
	}
	else if (label == "Position Drill")
	{
		if (value == "Not Conducted" || value == "No Workout")
		{
			curPlayer.mPositionDrill = 0;
		}
		else
		{
			int drillScore;
			sscanf_s(value,"%d score",&drillScore);
			curPlayer.mPositionDrill = drillScore;
		}
	}
	else if (label == "Developed")
	{
		int pctDeveloped;
		sscanf_s(value,"%d%%",&pctDeveloped);
		curPlayer.mPercentDeveloped = pctDeveloped;
	}
	else if (label == "Interviewed")
	{
		curPlayer.mInterviewed = value;
	}
	else if (label == "Impression")
	{
		curPlayer.mImpression = value;
	}
	else if (label == "Formations")
	{
		int formations = atoi(value);
		curPlayer.mFormations = (BYTE)(formations);
	}
	else if (label == "Loyalty")
	{
		curPlayer.mLoyalty = (BYTE)(GetDetailedNumber(value));
	}
	else if (label == "Wants Winner")
	{
		curPlayer.mWantsWinner = (BYTE)(GetDetailedNumber(value));
	}
	else if (label == "Leadership")
	{
		curPlayer.mLeadership = (BYTE)(GetDetailedNumber(value));
	}
	else if (label == "Intelligence")
	{
		curPlayer.mIntelligence = (BYTE)(GetDetailedNumber(value));
	}
	else if (label == "Personality")
	{
		curPlayer.mPersonality = (BYTE)(GetDetailedNumber(value));
	}
	else if (label == "Popularity")
	{
		curPlayer.mPopularity = (BYTE)(GetDetailedNumber(value));
	}
	else if (label == "Experience")
	{
		if (value == "R")
		{
			curPlayer.mExperience = 0;
		}
		else
		{
			int experience = atoi(value);
			curPlayer.mExperience = (BYTE)(experience);
		}
	}
	else if (label == "Mentor to")
	{
		curPlayer.mMentor = value;
	}
	else if (label == "Scouted Rating")
	{
		int cur;
		int fut;
		if (value.Find('/')>=0)
		{
			sscanf_s(value,"%d/%d",&cur,&fut);
		}
		else
		{
			fut = cur = atoi(value);
		}
		curPlayer.mCurrent = cur;
		curPlayer.mFuture = fut;
	}
	else if (label == "Character")
	{
		curPlayer.mCharacter = value;
	}
	else if (label == "Conflict with")
	{
		curPlayer.mConflicts = value;
	}
	else if (label == "Affinity with")
	{
		curPlayer.mAffinities = value;
	}
}

void ParseContractAmount(const CString& wndText, ULONG& amount)
{
	amount = 0;
	for (int i=0;i<wndText.GetLength();i++)
	{
		if (isdigit(wndText.GetAt(i)))
		{
			amount *= 10;
			amount += (wndText.GetAt(i)-'0');
		}
	}
}

HANDLE g_hSharedMemory = NULL;
LPTSTR g_pSharedMemory = NULL;
bool OpenSharedMemory()
{
	PSECURITY_DESCRIPTOR pSD = NULL; 
	pSD = HeapAlloc(GetProcessHeap(), 0, SECURITY_DESCRIPTOR_MIN_LENGTH);
	if(pSD == NULL)
	{
		DisplayError("Could not allocate security descriptor");
		return false;
	}

	// We now have an empty security descriptor
	if (!InitializeSecurityDescriptor(pSD,
		SECURITY_DESCRIPTOR_REVISION))
	{
		DisplayError("Could not initialize security descriptor");
		HeapFree(GetProcessHeap(), 0, pSD);
		return false;
	}

	if(!SetSecurityDescriptorDacl(pSD, TRUE, NULL, FALSE))
	{
		DisplayError("Could not set security descriptor DACL");
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
		DisplayError("Could not create shared memory");
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
		DisplayError("Could not create hook");
		return false;
	}

	Sleep(500);

	// Wait for the server window to be created.
	MSG msg;
	GetMessage(&msg,(HWND)-1,0,0);

 //   BOOL bClientConnected = ConnectNamedPipe(g_hSharedMemory, NULL);
	//if (!bClientConnected)
	//{
	//	if (GetLastError() != ERROR_PIPE_CONNECTED)
	//	{
	//		DisplayError("Client never connected to shared memory");
	//		return false;
	//	}
	//}

	// Find the handle of the hidden dialog box window.
	g_hwndCCA = ::FindWindow(NULL, TEXT("StelmackSoftCCA"));
	if (!IsWindow(g_hwndCCA))
	{
		CloseSharedMemory();
		DisplayError("Could not connect to CCA lib");
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
		DisplayError("Could not read from shared memory");
	}

	return retVal;
}

void CExtractorDlg::ProcessPlayerReportWindow(CWnd* reportWindow, bool appendToFile)
{
	CWnd* nextWindow = reportWindow->GetDlgItem(0x4b3);
	CListCtrl* scoutingReport = (CListCtrl*)reportWindow->GetDlgItem(0x4c4);
	//	CListCtrl* playerHonors = (CListCtrl*)reportWindow->GetDlgItem(0x4c8);
	CListCtrl* playerInfo1 = (CListCtrl*)reportWindow->GetDlgItem(0x5f0);
	//	CListCtrl* playerStats = (CListCtrl*)reportWindow->GetDlgItem(0x3f3);
	CListCtrl* playerInfo2 = (CListCtrl*)reportWindow->GetDlgItem(0x3f4);

	if (!scoutingReport || !playerInfo1 || !playerInfo2 || !nextWindow)
	{
		Sleep(250);
		nextWindow = reportWindow->GetDlgItem(0x4b3);
		scoutingReport = (CListCtrl*)reportWindow->GetDlgItem(0x4c4);
		playerInfo1 = (CListCtrl*)reportWindow->GetDlgItem(0x5f0);
		playerInfo2 = (CListCtrl*)reportWindow->GetDlgItem(0x3f4);
	}

	if (!scoutingReport || !playerInfo1 || !playerInfo2 || !nextWindow)
	{
		DisplayError("Could not read data on player report window");
		return;
	}

	if (!OpenCCAHook(reportWindow->GetSafeHwnd()))
	{
		return;
	}

	TCHAR playerNameBuf[1024];
	CString playerNameString;
	CString playerName;
	CString playerPosition;

	static const int kMaxPlayers = 4000;
	PlayerData playerData[kMaxPlayers];

	int playerIndex = 0;

	bool done = false;
	while (!done)
	{
		PlayerData& curPlayer = playerData[playerIndex];

		LRESULT charCount = reportWindow->SendDlgItemMessage(0x3f9,WM_GETTEXT,(WPARAM)1024,(LPARAM)playerNameBuf);
		playerNameBuf[charCount] = 0;
		bool hasNumber = (_istdigit(playerNameBuf[0])!=0);
		playerNameString = playerNameBuf;
		int lastSpacePos = playerNameString.ReverseFind(_T(' '));
		int firstSpacePos = playerNameString.Find(_T(' '));
		if (hasNumber)
		{
			// There are two spaces between the number and name
			playerName = playerNameString.Mid(firstSpacePos+2,lastSpacePos-firstSpacePos-3);
			CString jerseyString = playerNameString.Left(firstSpacePos);
			curPlayer.mJerseyNumber = (BYTE)atoi(jerseyString);
		}
		else
		{
			playerName = playerNameString.Left(lastSpacePos-1);
			curPlayer.mJerseyNumber = 0;
		}
		playerName.Replace(_T('\"'),_T('\''));
		mCurrentPlayerStatic.SetWindowText(playerNameBuf);
		playerPosition = playerNameString.Right(playerNameString.GetLength()-lastSpacePos-1);
		int statCount = gPositionAttributeCountMap[playerPosition];
		curPlayer.mName = playerName;
		curPlayer.mPosition = playerPosition;
		curPlayer.mPositionGroup = gPositionGroupMap[playerPosition];

		int itemIndex;
		int subItemIndex;
		CString itemData[2];

		for (itemIndex=0;itemIndex<playerInfo1->GetItemCount();++itemIndex)
		{
			for (subItemIndex=0;subItemIndex<2;++subItemIndex)
			{
				LPARAM lParam = MAKELPARAM(itemIndex,subItemIndex);
				::SendMessage(g_hwndCCA, CCAMSG_LV_GETITEMTEXT, (WPARAM)playerInfo1->m_hWnd, lParam);
				itemData[subItemIndex] = ReadSharedMemory();
			}
			ProcessLine(itemData[0],itemData[1],curPlayer);
		}

		for (itemIndex=0;itemIndex<playerInfo2->GetItemCount();++itemIndex)
		{
			for (subItemIndex=0;subItemIndex<2;++subItemIndex)
			{
				LPARAM lParam = MAKELPARAM(itemIndex,subItemIndex);
				::SendMessage(g_hwndCCA, CCAMSG_LV_GETITEMTEXT, (WPARAM)playerInfo2->m_hWnd, lParam);
				itemData[subItemIndex] = ReadSharedMemory();
			}
			ProcessLine(itemData[0],itemData[1],curPlayer);
		}

		CString reportData;
		for (itemIndex=0;itemIndex<scoutingReport->GetItemCount();++itemIndex)
		{
			// subitem 1 is scouted data
			// mod 100 is the current
			// div 100,000 is the additional growth available
			LPARAM lParam = MAKELPARAM(itemIndex,1);
			::SendMessage(g_hwndCCA, CCAMSG_LV_GETITEMTEXT, (WPARAM)scoutingReport->m_hWnd, lParam);
			reportData = ReadSharedMemory();
			unsigned int val = atoi(reportData);
			unsigned int lowVal = val%1000;
			unsigned int highVal = lowVal + (val/100000);
			curPlayer.mLowStats[itemIndex] = (BYTE)(lowVal);
			curPlayer.mHighStats[itemIndex] = (BYTE)(highVal);
		}

		playerIndex++;

		if (nextWindow->IsWindowEnabled())
		{
			::SendMessage(nextWindow->GetSafeHwnd(),BM_CLICK,0,0);
		}
		else
		{
			done = true;
		}
	}

	CloseCCAHook();

	mCurrentPlayerStatic.SetWindowText("Done - Writing File");

	std::ofstream outFile;
    TCHAR destPath[MAX_PATH];
    mOutputFilenameButton.GetWindowText(destPath,MAX_PATH);
	if (appendToFile)
	{
		outFile.open(destPath,std::ios::app);
	}
	else
	{
		outFile.open(destPath,std::ios::trunc);
	}
	outFile << std::setfill('0');

	if (!appendToFile)
	{
		outFile << "Name,Position,PosGrp,College,Team,Born,HomeTown,Agent,Designation";
		outFile << ",Height,Weight,Experience,Volatility,Jersey";
		outFile << ",Loyalty,Winner,Leader,Intelligence,Personality,Popularity,MentorTo";
		outFile << ",Solecismic,40Yard,Bench,Agility,BroadJump,PosDrill,PctDev,Intvwd,Impress";
		outFile << ",Cur,Fut,Conflicts,Affinities,Character";
		outFile << std::endl;
	}

	int playerCount = playerIndex;
	for (playerIndex=0;playerIndex<playerCount;playerIndex++)
	{
		outFile << "\"" << playerData[playerIndex].mName << "\"";
		outFile << ",\"" << playerData[playerIndex].mPosition << "\"";
		outFile << ",\"" << playerData[playerIndex].mPositionGroup << "\"";
		outFile << ",\"" << playerData[playerIndex].mCollege << "\"";
		outFile << ",\"" << playerData[playerIndex].mTeam << "\"";
		outFile << ",\"" << playerData[playerIndex].mBirthDate << "\"";
		outFile << ",\"" << playerData[playerIndex].mHometown << "\"";
		outFile << ",\"" << playerData[playerIndex].mAgent << "\"";
		outFile << ",\"" << playerData[playerIndex].mDesignation << "\"";
		outFile << "," << (int)playerData[playerIndex].mHeight;
		outFile << "," << (int)playerData[playerIndex].mWeight;
		outFile << "," << (int)playerData[playerIndex].mExperience;
		outFile << "," << (int)playerData[playerIndex].mVolatility;
		outFile << "," << (int)playerData[playerIndex].mJerseyNumber;
		outFile << "," << (int)playerData[playerIndex].mLoyalty;
		outFile << "," << (int)playerData[playerIndex].mWantsWinner;
		outFile << "," << (int)playerData[playerIndex].mLeadership;
		outFile << "," << (int)playerData[playerIndex].mIntelligence;
		outFile << "," << (int)playerData[playerIndex].mPersonality;
		outFile << "," << (int)playerData[playerIndex].mPopularity;
		outFile << "," << playerData[playerIndex].mMentor;
		outFile << "," << (int)playerData[playerIndex].mSolecismicTest;
		outFile << "," << (int)playerData[playerIndex].m40YardTimeWhole<<"."<< std::setw(2)<<(int)playerData[playerIndex].m40YardTimeFraction;
		outFile << "," << (int)playerData[playerIndex].mBenchPressReps;
		outFile << "," << (int)playerData[playerIndex].mAgilityWhole<<"."<< std::setw(2)<<(int)playerData[playerIndex].mAgilityFraction;
		outFile << "," << (int)(playerData[playerIndex].mBroadJumpFeet*12 + playerData[playerIndex].mBroadJumpInches);
		outFile << "," << (int)playerData[playerIndex].mPositionDrill;
		outFile << "," << (int)playerData[playerIndex].mPercentDeveloped;
		outFile << ",\"" << playerData[playerIndex].mInterviewed << "\"";
		outFile << ",\"" << playerData[playerIndex].mImpression << "\"";
		outFile << "," << (int)playerData[playerIndex].mCurrent;
		outFile << "," << (int)playerData[playerIndex].mFuture;
		outFile << ",\"" << playerData[playerIndex].mConflicts << "\"";
		outFile << ",\"" << playerData[playerIndex].mAffinities << "\"";
		outFile << ",\"" << playerData[playerIndex].mCharacter << "\"";
		if (playerData[playerIndex].mPosition == "QB")
		{
			outFile << "," << (int)playerData[playerIndex].mFormations;
		}
		for (int statIndex=0;statIndex<gPositionAttributeCountMap[playerData[playerIndex].mPosition];statIndex++)
		{
			outFile << "," << (int)playerData[playerIndex].mLowStats[statIndex];
			outFile << "," << (int)playerData[playerIndex].mHighStats[statIndex];
		}
		outFile << std::endl;
	}

	outFile.close();

	mCurrentPlayerStatic.SetWindowText("Finished!");
}

void CExtractorDlg::ProcessRosterWindow(CWnd* reportWindow)
{
	CComboBox* teamBox = static_cast<CComboBox*>(reportWindow->GetDlgItem(1127));
	CWnd* rosterList = reportWindow->GetDlgItem(1011);
	POINT clickPoint;
	// Should put us on the first item
	clickPoint.x = 10;
	clickPoint.y = 23;
	rosterList->ClientToScreen(&clickPoint);

	// Need screen coordinates to figure out the mapping
	int screenX = GetSystemMetrics(SM_CXSCREEN);
	int screenY = GetSystemMetrics(SM_CYSCREEN);
	// Convert to the 0-65K coordinates used by SendInput
	long clickX = (clickPoint.x*65535)/screenX;
	long clickY = (clickPoint.y*65535)/screenY;

	INPUT inputStruct;
	ZeroMemory(&inputStruct,sizeof(inputStruct));
	inputStruct.type = INPUT_MOUSE;

	// Dump all fields when doing this, since the first team listed (free agents)
	// may not have all the same data as all the other teams.
	gHaveDraftData = true;

	int startIndex = 1;
	if (mProcessFreeAgentsCheck.GetCheck() == BST_CHECKED)
	{
		startIndex = 0;
	}

	for (int i=startIndex;i<teamBox->GetCount();i++)
	{
		teamBox->SetCurSel(i);
		WPARAM wParam = MAKEWPARAM(1127,CBN_SELCHANGE);
		reportWindow->SendMessage(WM_COMMAND,wParam,(LPARAM)(teamBox->GetSafeHwnd()));
		Sleep(250);

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
		Sleep(250);

		// Move the mouse out of the way of the report window
		inputStruct.mi.dx = 0;
		inputStruct.mi.dy = 0;
		inputStruct.mi.dwFlags = MOUSEEVENTF_ABSOLUTE|MOUSEEVENTF_MOVE;
		SendInput(1,&inputStruct,sizeof(inputStruct));
		Sleep(50);

		// Now process the players for this team
		CWnd* playerWindow = FindWindow(NULL,"Player Report");
		if (!playerWindow)
		{
			Sleep(100);
			playerWindow = FindWindow(NULL,"Player Report");
		}
		if (!playerWindow)
		{
			Sleep(200);
			playerWindow = FindWindow(NULL,"Player Report");
		}
		if (playerWindow)
		{
			ProcessPlayerReportWindow(playerWindow,(i>startIndex));

			CWnd* exitWindow = playerWindow->GetDlgItem(1);
			::SendMessage(exitWindow->GetSafeHwnd(),BM_CLICK,0,0);
		}
		else
		{
			AfxMessageBox("The Player Report window failed to open. Aborting.",MB_OK|MB_ICONSTOP);
			return;
		}

		Sleep(100);
	}
}

void CExtractorDlg::ProcessScoutReportWindow(CWnd* reportWindow)
{
	CWnd* nextWindow = reportWindow->GetDlgItem(0x552);
	CListCtrl* reputationData = (CListCtrl*)reportWindow->GetDlgItem(0x3f3);
	CListCtrl* scoutInfo = (CListCtrl*)reportWindow->GetDlgItem(0x3f4);

	TCHAR scoutNameBuf[1024];

	static const int kMaxScouts = 500;
	ScoutData scoutData[kMaxScouts];

	int scoutIndex = 0;

	if (!OpenCCAHook(reportWindow->GetSafeHwnd()))
	{
		return;
	}

	bool done = false;
	while (!done)
	{
		ScoutData& curScout = scoutData[scoutIndex];
		curScout.mAskingPrice = 0;
		curScout.mContractYears = 0;

		LRESULT charCount = reportWindow->SendDlgItemMessage(0x3f9,WM_GETTEXT,(WPARAM)1024,(LPARAM)scoutNameBuf);
		scoutNameBuf[charCount] = 0;
		mCurrentPlayerStatic.SetWindowText(scoutNameBuf);
		curScout.mName = scoutNameBuf;

		int itemIndex;
		int subItemIndex;
		CString itemData[2];

		for (itemIndex=0;itemIndex<scoutInfo->GetItemCount();++itemIndex)
		{
			for (subItemIndex=0;subItemIndex<2;++subItemIndex)
			{
				LPARAM lParam = MAKELPARAM(itemIndex,subItemIndex);
				::SendMessage(g_hwndCCA, CCAMSG_LV_GETITEMTEXT, (WPARAM)scoutInfo->m_hWnd, lParam);
				itemData[subItemIndex] = ReadSharedMemory();
			}
			ProcessLine(itemData[0],itemData[1],curScout);
		}

		for (itemIndex=0;itemIndex<reputationData->GetItemCount();++itemIndex)
		{
			LPARAM lParam = MAKELPARAM(itemIndex,1);
			::SendMessage(g_hwndCCA, CCAMSG_LV_GETITEMTEXT, (WPARAM)reputationData->m_hWnd, lParam);
			int abilityScore = atoi(ReadSharedMemory());
			if (abilityScore >= 0 && abilityScore <= kMaxAbility)
			{
				curScout.mReputationData[itemIndex] = kAbilityMap[abilityScore];
			}
			else
			{
				curScout.mReputationData[itemIndex] = "Unknown";
			}
		}

		scoutIndex++;

		if (nextWindow->IsWindowEnabled())
		{
			::SendMessage(nextWindow->GetSafeHwnd(),BM_CLICK,0,0);
		}
		else
		{
			done = true;
		}
	}

	CloseCCAHook();

	mCurrentPlayerStatic.SetWindowText("Done - Writing File");

	TCHAR destPath[MAX_PATH];
	strcpy_s(destPath,gDestinationDirectory);
	PathAppend(destPath,"FOFScout.csv");
	std::ofstream outFile(destPath,std::ios::trunc);
	outFile << std::setfill('0');

	outFile << "Name,Team,Price,Years,Age,Playoffs,BowlWins,QB,RB,WR,OL,K,DL,LB,DB,Youth"<<std::endl;

	int scoutCount = scoutIndex;
	for (scoutIndex=0;scoutIndex<scoutCount;scoutIndex++)
	{
		outFile << scoutData[scoutIndex].mName;
		outFile << ","<< scoutData[scoutIndex].mCurrentTeam;
		outFile << ","<< scoutData[scoutIndex].mAskingPrice;
		outFile << ","<< (int)scoutData[scoutIndex].mContractYears;
		outFile << ","<< (int)scoutData[scoutIndex].mAge;
		outFile << ","<< (int)scoutData[scoutIndex].mPlayoffs;
		outFile << ","<< (int)scoutData[scoutIndex].mBowlWins;
		for (int i=0;i<kScoutReputationCount;i++)
		{
			outFile << ","<< scoutData[scoutIndex].mReputationData[i];
		}
		outFile << std::endl;
	}

	outFile.close();

	mCurrentPlayerStatic.SetWindowText("Finished!");
}

void CExtractorDlg::ProcessStaffReportWindow(CWnd* reportWindow)
{
	CWnd* nextWindow = reportWindow->GetDlgItem(0x552);
	CListCtrl* reputationData = (CListCtrl*)reportWindow->GetDlgItem(0x3f3);
	CListCtrl* coachInfo = (CListCtrl*)reportWindow->GetDlgItem(0x3f4);

	TCHAR coachNameBuf[1024];

	static const int kMaxCoaches = 500;
	CoachData coachData[kMaxCoaches];

	int coachIndex = 0;

	if (!OpenCCAHook(reportWindow->GetSafeHwnd()))
	{
		return;
	}

	bool done = false;
	while (!done)
	{
		CoachData& curCoach = coachData[coachIndex];
		curCoach.mAskingPrice = 0;
		curCoach.mContractYears = 0;

		LRESULT charCount = reportWindow->SendDlgItemMessage(0x3f9,WM_GETTEXT,(WPARAM)1024,(LPARAM)coachNameBuf);
		coachNameBuf[charCount] = 0;
		mCurrentPlayerStatic.SetWindowText(coachNameBuf);
		curCoach.mName = coachNameBuf;

		int itemIndex;
		int subItemIndex;
		CString itemData[2];

		for (itemIndex=0;itemIndex<coachInfo->GetItemCount();++itemIndex)
		{
			for (subItemIndex=0;subItemIndex<2;++subItemIndex)
			{
				LPARAM lParam = MAKELPARAM(itemIndex,subItemIndex);
				::SendMessage(g_hwndCCA, CCAMSG_LV_GETITEMTEXT, (WPARAM)coachInfo->m_hWnd, lParam);
				itemData[subItemIndex] = ReadSharedMemory();
			}
			ProcessLine(itemData[0],itemData[1],curCoach);
		}

		for (itemIndex=0;itemIndex<reputationData->GetItemCount();++itemIndex)
		{
			LPARAM lParam = MAKELPARAM(itemIndex,1);
			::SendMessage(g_hwndCCA, CCAMSG_LV_GETITEMTEXT, (WPARAM)reputationData->m_hWnd, lParam);
			int abilityScore = atoi(ReadSharedMemory());
			if (abilityScore >= 0 && abilityScore <= kMaxAbility)
			{
				curCoach.mReputationData[itemIndex] = kAbilityMap[abilityScore];
			}
			else
			{
				curCoach.mReputationData[itemIndex] = "Unknown";
			}
		}

		coachIndex++;

		if (nextWindow->IsWindowEnabled())
		{
			::SendMessage(nextWindow->GetSafeHwnd(),BM_CLICK,0,0);
		}
		else
		{
			done = true;
		}
	}

	CloseCCAHook();

	mCurrentPlayerStatic.SetWindowText("Done - Writing File");

	TCHAR destPath[MAX_PATH];
	strcpy_s(destPath,gDestinationDirectory);
	PathAppend(destPath,"FOFCoach.csv");
	std::ofstream outFile(destPath,std::ios::trunc);
	outFile << std::setfill('0');

	outFile << "Name,Position,Team,Price,Years,Age,Exp,Playoffs,BowlWins,QB,RB,WR,OL,K,DL,LB,DB,Youth,Motiv,Disc,Off,Def,Inj"<<std::endl;

	int coachCount = coachIndex;
	for (coachIndex=0;coachIndex<coachCount;coachIndex++)
	{
		outFile << coachData[coachIndex].mName;
		outFile << ","<< coachData[coachIndex].mPosition;
		outFile << ","<< coachData[coachIndex].mCurrentTeam;
		outFile << ","<< coachData[coachIndex].mAskingPrice;
		outFile << ","<< (int)coachData[coachIndex].mContractYears;
		outFile << ","<< (int)coachData[coachIndex].mAge;
		outFile << ","<< (int)coachData[coachIndex].mExperience;
		outFile << ","<< (int)coachData[coachIndex].mPlayoffs;
		outFile << ","<< (int)coachData[coachIndex].mBowlWins;
		for (int i=0;i<kCoachReputationCount;i++)
		{
			outFile << ","<< coachData[coachIndex].mReputationData[i];
		}
		outFile << std::endl;
	}

	outFile.close();

	mCurrentPlayerStatic.SetWindowText("Finished!");
}

void DoItemClick(CListCtrl* listCtrl)
{
	// Need screen coordinates to figure out the mapping
	int screenX = GetSystemMetrics(SM_CXSCREEN);
	int screenY = GetSystemMetrics(SM_CYSCREEN);

	CString result = ReadSharedMemory();
	RECT itemRect;
	int curPos = 0;
	CString curToken = result.Tokenize(",",curPos);
	itemRect.left = atoi(curToken);
	curToken = result.Tokenize(",",curPos);
	itemRect.top = atoi(curToken);
	curToken = result.Tokenize(",",curPos);
	itemRect.right = atoi(curToken);
	curToken = result.Tokenize(",",curPos);
	itemRect.bottom = atoi(curToken);
	POINT clickPoint;
	clickPoint.x = (itemRect.left + itemRect.right)/2;
	clickPoint.y = (itemRect.top + itemRect.bottom)/2;
	listCtrl->ClientToScreen(&clickPoint);
	// Convert to the 0-65K coordinates used by SendInput
	long mouseX = (clickPoint.x*65535)/screenX;
	long mouseY = (clickPoint.y*65535)/screenY;

	INPUT inputStruct;
	ZeroMemory(&inputStruct,sizeof(inputStruct));
	inputStruct.type = INPUT_MOUSE;
	inputStruct.mi.dx = mouseX;
	inputStruct.mi.dy = mouseY;

	inputStruct.mi.dwFlags = MOUSEEVENTF_ABSOLUTE|MOUSEEVENTF_MOVE;
	SendInput(1,&inputStruct,sizeof(inputStruct));
	Sleep(50);
	inputStruct.mi.dwFlags = MOUSEEVENTF_ABSOLUTE|MOUSEEVENTF_LEFTDOWN;
	SendInput(1,&inputStruct,sizeof(inputStruct));
	Sleep(50);
	inputStruct.mi.dwFlags = MOUSEEVENTF_ABSOLUTE|MOUSEEVENTF_LEFTUP;
	SendInput(1,&inputStruct,sizeof(inputStruct));
	Sleep(250);
}

void ClickCenterOfItem(CListCtrl* listCtrl, int itemIndex)
{
	LPARAM lParam = MAKELPARAM(itemIndex,0);
	::SendMessage(g_hwndCCA, CCAMSG_LV_GETITEMRECT, (WPARAM)listCtrl->m_hWnd, lParam);
	DoItemClick(listCtrl);
}

void ClickCenterOfSubItem(CListCtrl* listCtrl, int itemIndex, int subItemIndex)
{
	LPARAM lParam = MAKELPARAM(itemIndex,subItemIndex);
	::SendMessage(g_hwndCCA, CCAMSG_LV_GETSUBITEMRECT, (WPARAM)listCtrl->m_hWnd, lParam);
	DoItemClick(listCtrl);
}

struct TeamPowerRating
{
	CString teamName;
	int rating;
	double winPct;
	double oppPct;
};
std::vector<TeamPowerRating> gPowerRatings;
struct GameEntry
{
	CString awayTeam;
	CString homeTeam;
	CString awayTeamRecord;
	CString homeTeamRecord;
	int awayTeamPowerRating;
	int homeTeamPowerRating;
	double gameRating;
	int pointspread;	// how many points away team gives (will be positive if home favored)
	int precip;
	int temperature;
	int windSpeed;

	friend bool operator<(const GameEntry& left, const GameEntry& right);
};
bool operator<(const GameEntry& left, const GameEntry& right)
{
	return left.gameRating < right.gameRating;
}
std::vector<GameEntry> gGameEntries;

void CExtractorDlg::GrabPowerRatings(CWnd* ratingsWindow)
{
	gPowerRatings.clear();
	ratingsWindow->SetActiveWindow();
	Sleep(50);

	CListCtrl* ratingsList = (CListCtrl*)ratingsWindow->GetDlgItem(0x3f3);
	if (!ratingsList)
	{
		AfxMessageBox("Could not find the list of ratings in the Power Ratings window."
			,MB_OK|MB_ICONSTOP);
		return;
	}

	for (int itemIndex=0;itemIndex<ratingsList->GetItemCount();++itemIndex)
	{
		TeamPowerRating powerRating;
		LPARAM lParam = MAKELPARAM(itemIndex,0);
		::SendMessage(g_hwndCCA, CCAMSG_LV_GETITEMTEXT, (WPARAM)ratingsList->m_hWnd, lParam);
		powerRating.teamName = ReadSharedMemory();
		lParam = MAKELPARAM(itemIndex,2);
		::SendMessage(g_hwndCCA, CCAMSG_LV_GETITEMTEXT, (WPARAM)ratingsList->m_hWnd, lParam);
		powerRating.winPct = atof(ReadSharedMemory());
		lParam = MAKELPARAM(itemIndex,3);
		::SendMessage(g_hwndCCA, CCAMSG_LV_GETITEMTEXT, (WPARAM)ratingsList->m_hWnd, lParam);
		powerRating.rating = atoi(ReadSharedMemory());
		lParam = MAKELPARAM(itemIndex,5);
		::SendMessage(g_hwndCCA, CCAMSG_LV_GETITEMTEXT, (WPARAM)ratingsList->m_hWnd, lParam);
		powerRating.oppPct = atof(ReadSharedMemory());
		gPowerRatings.push_back(powerRating);
	}

	::SendMessage(ratingsWindow->GetSafeHwnd(),WM_CLOSE,0,0);
	Sleep(50);
}

void ParseTeam(const CString& teamString, CString& teamName, CString& teamRecord, int& pointSpread)
{
	pointSpread = 1000;
	int recordStart = teamString.Find(" (");
	if (recordStart < 0)
	{
		AfxMessageBox("Could not find record start for game."
			,MB_OK|MB_ICONSTOP);
		return;
	}
	teamName = teamString.Left(recordStart);
	teamName.Remove('#');
	teamName.Remove('%');
	recordStart += 2;
	int recordEnd = teamString.Find(")",recordStart);
	if (recordEnd < 0)
	{
		AfxMessageBox("Could not find record end for game."
			,MB_OK|MB_ICONSTOP);
		return;
	}
	teamRecord = teamString.Mid(recordStart,recordEnd-recordStart);

	int spreadStart = teamString.Find("(",recordEnd);
	if (spreadStart >= 0)
	{
		spreadStart += 1;
		int spreadEnd = teamString.Find(")",spreadStart);
		if (spreadEnd >= 0)
		{
			CString spreadString = teamString.Mid(spreadStart,spreadEnd-spreadStart);
			if (spreadString == "E")
			{
				pointSpread = 0;
			}
			else
			{
				pointSpread = atoi(spreadString);
			}
		}
	}
}

void ParseMatchup(const CString& matchup, GameEntry& gameEntry)
{
	int teamSplit = matchup.Find(" at ");
	if (teamSplit < 0)
	{
		teamSplit = matchup.Find(" vs ");
	}
	if (teamSplit < 0)
	{
		AfxMessageBox("Could not find team split in the game list."
			,MB_OK|MB_ICONSTOP);
		return;
	}

	CString teamName;
	CString teamRecord;
	int pointSpread;
	
	CString teamString = matchup.Left(teamSplit);
	ParseTeam(teamString,teamName,teamRecord,pointSpread);
	gameEntry.awayTeam = teamName;
	gameEntry.awayTeamRecord = teamRecord;
	if (pointSpread <= 0)
	{
		gameEntry.pointspread = pointSpread;
	}
	teamString = matchup.Mid(teamSplit+4);
	ParseTeam(teamString,teamName,teamRecord,pointSpread);
	gameEntry.homeTeam = teamName;
	gameEntry.homeTeamRecord = teamRecord;
	if (pointSpread <= 0)
	{
		gameEntry.pointspread = -pointSpread;
	}
}

const TeamPowerRating& FindPowerRating(const CString& teamName)
{
	static TeamPowerRating defaultRating = { "",0,0.0,0.0 };
	for (size_t ratingIndex=0;ratingIndex<gPowerRatings.size();++ratingIndex)
	{
		if (gPowerRatings[ratingIndex].teamName.Find(teamName) >= 0)
		{
			return gPowerRatings[ratingIndex];
		}
	}
	return defaultRating;
}

void CExtractorDlg::GrabWeeklySchedule(CWnd* scheduleWindow)
{
	gGameEntries.clear();

	scheduleWindow->SetActiveWindow();
	Sleep(50);

	CListCtrl* gameList = (CListCtrl*)scheduleWindow->GetDlgItem(0x3f3);
	if (!gameList)
	{
		AfxMessageBox("Could not find the list of games in the Schedule Window."
			,MB_OK|MB_ICONSTOP);
		return;
	}

	for (int itemIndex=0;itemIndex<gameList->GetItemCount();++itemIndex)
	{
		GameEntry gameEntry;
		LPARAM lParam = MAKELPARAM(itemIndex,0);
		::SendMessage(g_hwndCCA, CCAMSG_LV_GETITEMTEXT, (WPARAM)gameList->m_hWnd, lParam);
		CString matchup = ReadSharedMemory();
		ParseMatchup(matchup, gameEntry);

		lParam = MAKELPARAM(itemIndex,1);
		::SendMessage(g_hwndCCA, CCAMSG_LV_GETITEMTEXT, (WPARAM)gameList->m_hWnd, lParam);
		int weather = atoi(ReadSharedMemory());
		gameEntry.precip = weather / 7800;
		gameEntry.windSpeed = ((weather % 7800) / 120);
		gameEntry.temperature = ((weather % 120) - 10);

		gGameEntries.push_back(gameEntry);
	}

	::SendMessage(scheduleWindow->GetSafeHwnd(),WM_CLOSE,0,0);
	Sleep(50);
}

static CString kPrecipMap[] =
{
	"Fair"
	,"Rain"
	,"Stormy"
	,"Snow"
};

static const int kGameStyleCount = 16;
static const char* kGameStyleStrings[kGameStyleCount] = 
{
	"background-color: #888888; color:White; font-size: 14px; font-family: Arial;"
	,"background-color: #666666; color:White; font-size: 14px; font-family: Arial;"
	,"background-color: #666666; color:White; font-size: 14px; font-family: Arial;"
	,"background-color: #666666; color:White; font-size: 14px; font-family: Arial;"
	,"background-color: #666666; color:White; font-size: 14px; font-family: Arial;"
	,"background-color: #444444; color:White; font-size: 14px; font-family: Arial;"
	,"background-color: #444444; color:White; font-size: 14px; font-family: Arial;"
	,"background-color: #444444; color:White; font-size: 14px; font-family: Arial;"
	,"background-color: #444444; color:White; font-size: 14px; font-family: Arial;"
	,"background-color: #444444; color:White; font-size: 14px; font-family: Arial;"
	,"background-color: #444444; color:White; font-size: 14px; font-family: Arial;"
	,"background-color: #444444; color:White; font-size: 14px; font-family: Arial;"
	,"background-color: #444444; color:White; font-size: 14px; font-family: Arial;"
	,"background-color: #444444; color:White; font-size: 14px; font-family: Arial;"
	,"background-color: #444444; color:White; font-size: 14px; font-family: Arial;"
	,"background-color: #444444; color:White; font-size: 14px; font-family: Arial;"
};

static int gCurGameStyle = 0;

void WriteGame(std::ofstream& outFile, const GameEntry& entry,const char* description)
{
	outFile << "        <tr style=\""<<kGameStyleStrings[gCurGameStyle]<<"\">\n";
	outFile << "            <td><img src=\"" << gTeamImageMap[entry.awayTeam]<<"\" /></td>\n";
	outFile << "            <td style=\"text-align: center\"><b>"<<description<<":</b> "
		<< entry.temperature<<" degrees, "<<entry.windSpeed<<"MPH, "<<kPrecipMap[entry.precip]<<"<br />\n";
	outFile << "            <b>"<<entry.awayTeam<<"</b> ( "<<entry.awayTeamRecord<<", PR "<<entry.awayTeamPowerRating<<" )";
	outFile << " at ";
	outFile << "<b>" << entry.homeTeam <<"</b> ( "<<entry.homeTeamRecord<<", PR "<<entry.homeTeamPowerRating<<" )<br />\n";
	if (entry.pointspread < 0)
	{
		outFile << entry.awayTeam << " favored by "<<-entry.pointspread;
	}
	else if (entry.pointspread > 0)
	{
		outFile << entry.homeTeam << " favored by "<<entry.pointspread;
	}
	else if (entry.pointspread == 0)
	{
		outFile << "PUSH";
	}
	outFile << "</td>\n";
	outFile << "            <td><img src=\"" << gTeamImageMap[entry.homeTeam]<<"\" /></td>\n";
	outFile << "        </tr>\n";

	gCurGameStyle = (gCurGameStyle + 1)%kGameStyleCount;
}

void GenerateHTMLForGames()
{
	gCurGameStyle = 0;

	std::ofstream outFile;
	TCHAR destPath[MAX_PATH];
	strcpy_s(destPath,gDestinationDirectory);
	PathAppend(destPath,"WeeklyGames.html");
	outFile.open(destPath,std::ios::trunc);

	std::priority_queue<GameEntry> gameQueue;
	for (size_t gameIndex=0;gameIndex<gGameEntries.size();++gameIndex)
	{
		gameQueue.push(gGameEntries[gameIndex]);
	}

	outFile << "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\n";
	outFile << "<html xmlns=\"http://www.w3.org/1999/xhtml\" >\n";
	outFile << "<body>\n";
	outFile << "    <table border=\"1\" cellpadding=\"4\">\n";
	if (gameQueue.size() <= 4)
	{
		if (!gameQueue.empty())
		{
			WriteGame(outFile,gameQueue.top(),"Marquee Matchup");
			gameQueue.pop();
		}
		if (!gameQueue.empty())
		{
			WriteGame(outFile,gameQueue.top(),"Game 2");
			gameQueue.pop();
		}
		if (!gameQueue.empty())
		{
			WriteGame(outFile,gameQueue.top(),"Game 3");
			gameQueue.pop();
		}
		if (!gameQueue.empty())
		{
			WriteGame(outFile,gameQueue.top(),"Game 4");
			gameQueue.pop();
		}
	}
	else
	{
		if (!gameQueue.empty())
		{
			WriteGame(outFile,gameQueue.top(),"Monday Night Game");
			gameQueue.pop();
		}
		if (!gameQueue.empty())
		{
			WriteGame(outFile,gameQueue.top(),"Sunday Night Game");
			gameQueue.pop();
		}
		if (!gameQueue.empty())
		{
			WriteGame(outFile,gameQueue.top(),"Thursday Night Game");
			gameQueue.pop();
		}
		if (!gameQueue.empty())
		{
			WriteGame(outFile,gameQueue.top(),"Saturday Late Game");
			gameQueue.pop();
		}
		if (!gameQueue.empty())
		{
			WriteGame(outFile,gameQueue.top(),"Saturday Early Game");
			gameQueue.pop();
		}
		while (!gameQueue.empty())
		{
			WriteGame(outFile,gameQueue.top(),"Sunday Game");
			gameQueue.pop();
		}
	}
	outFile << "    </table>\n";
	outFile << "</body>\n";
	outFile << "</html>\n";
	outFile.close();

	::ShellExecute(NULL,"open",destPath,NULL,NULL,SW_SHOWNORMAL);
}

static HWND gSimulationWindow;
BOOL CALLBACK FindSimulationWindowProc(HWND hwndChild, LPARAM lParam) 
{
	TCHAR windowName[MAX_PATH];
	GetWindowText(hwndChild,windowName,MAX_PATH);
	if (_tcscmp(windowName,"Simulation Window") == 0)
	{
		gSimulationWindow = hwndChild;
		return FALSE;
	}
	else
	{
	    return TRUE;
	}
}

void CExtractorDlg::ProcessWeeklySchedule(CWnd* reportWindow)
{
	CWnd* fofWindow = FindWindow(NULL,"Front Office Football 2007");
	if (!fofWindow)
	{
		AfxMessageBox("Could not find the FOF Window???????"
			,MB_OK|MB_ICONSTOP);
		return;
	}
	gSimulationWindow = false;
	EnumChildWindows(fofWindow->GetSafeHwnd(), FindSimulationWindowProc, NULL); 

	//CWnd* afxFrame = fofWindow->GetDescendantWindow(0xe900);
	//if (!afxFrame)
	//{
	//	AfxMessageBox("Could not find the AFX Frame window."
	//		,MB_OK|MB_ICONSTOP);
	//	return;
	//}

	//CWnd* simWindow = FindWindowEx(afxFrame->GetSafeHwnd(),NULL,NULL,"Simulation Window");
	//if (!simWindow)
	if (!gSimulationWindow)
	{
		AfxMessageBox("Could not find the Simulation Window.\n"
			"Open it up and try running again. If the Simulation Window is open,\n"
			"try clicking the simulation and then clicking Extract again."
			,MB_OK|MB_ICONSTOP);
		return;
	}
	CWnd* simWindow = CWnd::FromHandle(gSimulationWindow);
	CListCtrl* commandList = (CListCtrl*)simWindow->GetDlgItem(0x4c8);
	if (!commandList)
	{
		AfxMessageBox("Could not find the list of commands in the Simulation Window.\n"
			,MB_OK|MB_ICONSTOP);
		return;
	}

	OpenCCAHook(fofWindow->GetSafeHwnd());

	CString windowName;
	reportWindow->GetWindowText(windowName);
	if (windowName == "Power Ratings")
	{
		GrabPowerRatings(reportWindow);

		ClickCenterOfItem(commandList,0);

		CWnd* scheduleWindow;
		for (scheduleWindow = GetWindow(GW_HWNDFIRST);scheduleWindow != NULL;scheduleWindow = scheduleWindow->GetWindow(GW_HWNDNEXT))
		{
			scheduleWindow->GetWindowText(windowName);
			if (windowName.Find(" Season, ")>= 0)
			{
				break;
			}
		}
		if (!scheduleWindow)
		{
			AfxMessageBox("Weekly Schedule window did not open.\n"
				,MB_OK|MB_ICONSTOP);
			return;
		}
		GrabWeeklySchedule(scheduleWindow);
	}
	else
	{
		GrabWeeklySchedule(reportWindow);

		ClickCenterOfItem(commandList,6);

		CWnd* ratingsWindow = FindWindow(NULL,"Power Ratings");
		if (!ratingsWindow)
		{
			AfxMessageBox("Power Ratings window did not open.\n"
				,MB_OK|MB_ICONSTOP);
			return;
		}
		GrabPowerRatings(ratingsWindow);
	}

	CloseCCAHook();

	std::ofstream ratingLogFile;
	TCHAR destPath[MAX_PATH];
	strcpy_s(destPath,gDestinationDirectory);
	PathAppend(destPath,"WeeklyGames.log");
	ratingLogFile.open(destPath,std::ios::trunc);

	for (size_t gameIndex=0;gameIndex<gGameEntries.size();++gameIndex)
	{
		const TeamPowerRating& awayRating = FindPowerRating(gGameEntries[gameIndex].awayTeam);
		const TeamPowerRating& homeRating = FindPowerRating(gGameEntries[gameIndex].homeTeam);
		double awayScore = awayRating.winPct + (0.0075 * (double)awayRating.rating) + (0.5 * awayRating.oppPct);
		double homeScore = homeRating.winPct + (0.0075 * (double)homeRating.rating) + (0.5 * homeRating.oppPct);
		double scoreSum = awayScore + homeScore;
		double scoreDiff = fabs(awayScore - homeScore);
		double adjScoreDiff = scoreDiff * 0.5;
		double gameScore = scoreSum - adjScoreDiff;
		ratingLogFile << "********************\n";
		ratingLogFile << "Away Team: " << gGameEntries[gameIndex].awayTeam << " score=" << awayScore << " winPct=" << awayRating.winPct
			<< " oppPct=" << awayRating.oppPct << " power=" << awayRating.rating << "\n";
		ratingLogFile << "Home Team: " << gGameEntries[gameIndex].homeTeam << " score=" << homeScore << " winPct=" << homeRating.winPct
			<< " oppPct=" << homeRating.oppPct << " power=" << homeRating.rating << "\n";
		ratingLogFile << "Game Score = " << gameScore << " (sum = " << scoreSum << " diff = " << scoreDiff << " adj diff = "
			<< adjScoreDiff << ")\n";
		ratingLogFile << "********************\n";
		gGameEntries[gameIndex].gameRating = gameScore;
		gGameEntries[gameIndex].awayTeamPowerRating = awayRating.rating;
		gGameEntries[gameIndex].homeTeamPowerRating = homeRating.rating;
	}

	ratingLogFile.close();

	GenerateHTMLForGames();

	mCurrentPlayerStatic.SetWindowText("Done grabbing schedule!");
}

void CExtractorDlg::OnBnClickedExtractButton()
{
	gHaveDraftData = false;

	CWnd* reportWindow = FindWindow(NULL,"Player Report");
	if (reportWindow)
	{
		ProcessPlayerReportWindow(reportWindow,false);
		return;
	}
	reportWindow = FindWindow(NULL,"View Scout");
	if (reportWindow)
	{
		ProcessScoutReportWindow(reportWindow);
		return;
	}
	reportWindow = FindWindow(NULL,"View Staff Member");
	if (reportWindow)
	{
		ProcessStaffReportWindow(reportWindow);
		return;
	}

	CString windowName;
	bool rosterWindow = false;
	bool scheduleWindow = false;
	bool powerRatingsWindow = false;
	for (reportWindow = GetWindow(GW_HWNDFIRST);reportWindow != NULL;reportWindow = reportWindow->GetWindow(GW_HWNDNEXT))
	{
		reportWindow->GetWindowText(windowName);
		if (   windowName.Left(17) == "From the desk of "
			|| windowName.Right(7) == " Roster"
			|| windowName.Right(18) == " Contract Overview"
			|| windowName.Right(17) == " Personality View"
			|| windowName.Right(14) == " Injury Report"
			|| windowName.Right(18) == " Attitude Advisory"
			)
		{
			rosterWindow = true;
			break;
		}
		else if (windowName.Find(" Season, ")>= 0)
		{
			scheduleWindow = true;
			break;
		}
		else if (windowName == "Power Ratings")
		{
			powerRatingsWindow = true;
			break;
		}
	}
	if (reportWindow)
	{
		if (rosterWindow)
		{
			ProcessRosterWindow(reportWindow);
			return;
		}
		else if (scheduleWindow || powerRatingsWindow)
		{
			ProcessWeeklySchedule(reportWindow);
			return;
		}
	}

	AfxMessageBox("Could not find any Windows\n"
		"Make sure you've opened FOF, and opened either a player report,\n"
		" a scout, a staff member, power ratings, or simulate games.",MB_OK|MB_ICONSTOP);
	return;
}


void CExtractorDlg::OnBnClickedButtonFilename()
{
    TCHAR szFilters[]= _T("CSV Files (*.csv)|*.csv|All Files (*.*)|*.*||");

    CFileDialog fileDlg(FALSE,_T("csv"),_T("FOFRoster.csv"),OFN_CREATEPROMPT|OFN_NOREADONLYRETURN|OFN_OVERWRITEPROMPT|OFN_PATHMUSTEXIST,
        szFilters);
    fileDlg.GetOFN().lpstrInitialDir = gDestinationDirectory;

    if(fileDlg.DoModal() == IDOK)
    {
        CString pathName = fileDlg.GetPathName();

        mOutputFilenameButton.SetWindowText(pathName);
    }
}
