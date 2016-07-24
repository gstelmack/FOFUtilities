
// TicketPriceSetterDlg.cpp : implementation file
//

#include "stdafx.h"
#include "TicketPriceSetter.h"
#include "TicketPriceSetterDlg.h"
#include "afxdialogex.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CTicketPriceSetterDlg dialog



CTicketPriceSetterDlg::CTicketPriceSetterDlg(CWnd* pParent /*=NULL*/)
	: CDialogEx(CTicketPriceSetterDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CTicketPriceSetterDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CTicketPriceSetterDlg, CDialogEx)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BUTTON_REX, &CTicketPriceSetterDlg::OnBnClickedButtonRex)
END_MESSAGE_MAP()


// CTicketPriceSetterDlg message handlers

BOOL CTicketPriceSetterDlg::OnInitDialog()
{
	CDialogEx::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	// TODO: Add extra initialization here

	return TRUE;  // return TRUE  unless you set the focus to a control
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CTicketPriceSetterDlg::OnPaint()
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
		CDialogEx::OnPaint();
	}
}

// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CTicketPriceSetterDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}



void CTicketPriceSetterDlg::OnBnClickedButtonRex()
{
	CWnd* fofWindow = FindWindow(NULL,TEXT("Front Office Football Seven"));
	if (!fofWindow)
	{
		AfxMessageBox(TEXT("Cannot FOF window.\n"), MB_OK | MB_ICONSTOP);
		return;
	}
	fofWindow = fofWindow->GetWindow(GW_CHILD);

	CString windowName;
	CWnd* gameOptionsWindow = NULL;
	CWnd* almanacWindow = NULL;
	CWnd* testWindow;
	for (testWindow = fofWindow->GetWindow(GW_CHILD); testWindow != NULL; testWindow = testWindow->GetWindow(GW_HWNDNEXT))
	{
		testWindow->GetWindowText(windowName);
		if (windowName == "Game Options")
		{
			gameOptionsWindow = testWindow;
		}
		else if (windowName == "Almanac")
		{
			almanacWindow = testWindow;
		}
	}
	if (!gameOptionsWindow)
	{
		AfxMessageBox(TEXT("Cannot find Game Options window.\n"), MB_OK | MB_ICONSTOP);
		return;
	}

	CWnd* gameOptionsList = gameOptionsWindow->GetDlgItem(0x3f3);
	if (!gameOptionsList)
	{
		AfxMessageBox(TEXT("No game options list in game options window?\n"), MB_OK | MB_ICONSTOP);
		return;
	}

	if (!almanacWindow)
	{
		AfxMessageBox(TEXT("Cannot find Almanac window.\n"), MB_OK | MB_ICONSTOP);
		return;
	}
	CWnd* almanacList = almanacWindow->GetDlgItem(0x3f3);
	if (!almanacList)
	{
		AfxMessageBox(TEXT("No almanac list in almanac window?\n"), MB_OK | MB_ICONSTOP);
		return;
	}

	// Need screen coordinates to figure out the mapping
	int screenX = GetSystemMetrics(SM_CXSCREEN);
	int screenY = GetSystemMetrics(SM_CYSCREEN);

	for (int teamIndex = 0; teamIndex < 32; ++teamIndex)
	{
		// Click change color
		DoMouseClick(26, 98, gameOptionsList, screenX, screenY);

		// Change the color to the correct team
		CWnd* colorChooserWindow = FindWindow(NULL, TEXT("Color Chooser"));
		if (!colorChooserWindow)
		{
			AfxMessageBox(TEXT("Color chooser did not open.\n"), MB_OK | MB_ICONSTOP);
			return;
		}
		CComboBox* teamBox = static_cast<CComboBox*>(colorChooserWindow->GetDlgItem(0x467));
		if (!teamBox)
		{
			AfxMessageBox(TEXT("No team box in color chooser?\n"), MB_OK | MB_ICONSTOP);
			return;
		}
		teamBox->SetCurSel(teamIndex);
		WPARAM wParam = MAKEWPARAM(1127, CBN_SELCHANGE);
		colorChooserWindow->SendMessage(WM_COMMAND, wParam, (LPARAM)(teamBox->GetSafeHwnd()));
		Sleep(250);

		// Close the dialog
		CWnd* saveButton = colorChooserWindow->GetDlgItem(1);
		if (!saveButton)
		{
			AfxMessageBox(TEXT("No save button in color chooser?\n"), MB_OK | MB_ICONSTOP);
			return;
		}
		::SendMessage(saveButton->GetSafeHwnd(), BM_CLICK, 0, 0);
		Sleep(250);

		// Click Stadium options
		DoMouseClick(43, 443, almanacList, screenX, screenY);

		// Click recommend prices
		CWnd* ticketPricesWindow = FindWindow(NULL, TEXT("Ticket Prices and Stadium Information"));
		if (!ticketPricesWindow)
		{
			AfxMessageBox(TEXT("Ticket prices did not open.\n"), MB_OK | MB_ICONSTOP);
			return;
		}
		CWnd* recommendButton = ticketPricesWindow->GetDlgItem(0x58d);
		if (!recommendButton)
		{
			AfxMessageBox(TEXT("No recommend button in ticket prices window?\n"), MB_OK | MB_ICONSTOP);
			return;
		}
		::SendMessage(recommendButton->GetSafeHwnd(), BM_CLICK, 0, 0);
		Sleep(250);

		// Close the dialog
		CWnd* okbutton = ticketPricesWindow->GetDlgItem(1);
		if (!okbutton)
		{
			AfxMessageBox(TEXT("No OK button in ticket prices window?\n"), MB_OK | MB_ICONSTOP);
			return;
		}
		::SendMessage(okbutton->GetSafeHwnd(), BM_CLICK, 0, 0);
		Sleep(250);
	}
}

void CTicketPriceSetterDlg::DoMouseClick(int x, int y, CWnd* window, int screenX, int screenY)
{
	INPUT inputStruct;
	ZeroMemory(&inputStruct, sizeof(inputStruct));
	POINT clickPoint;
	inputStruct.type = INPUT_MOUSE;
	clickPoint.x = x;
	clickPoint.y = y;
	window->ClientToScreen(&clickPoint);
	long clickX = (clickPoint.x * 65535) / screenX;
	long clickY = (clickPoint.y * 65535) / screenY;

	inputStruct.mi.dx = clickX;
	inputStruct.mi.dy = clickY;

	inputStruct.mi.dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE;
	SendInput(1, &inputStruct, sizeof(inputStruct));
	Sleep(50);
	inputStruct.mi.dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTDOWN;
	SendInput(1, &inputStruct, sizeof(inputStruct));
	Sleep(50);
	inputStruct.mi.dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTUP;
	SendInput(1, &inputStruct, sizeof(inputStruct));
	Sleep(250);
}