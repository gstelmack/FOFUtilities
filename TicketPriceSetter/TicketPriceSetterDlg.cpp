
// TicketPriceSetterDlg.cpp : implementation file
//

#include "stdafx.h"
#include "TicketPriceSetter.h"
#include "TicketPriceSetterDlg.h"
#include "afxdialogex.h"

#include <sstream>

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
	CDialogEx::DoDataExchange( pDX );
	DDX_Control( pDX, IDC_STATIC_MOUSE_POSITION, m_MousePosition );
	DDX_Control( pDX, IDC_EDIT_MOUSEX, m_MouseX );
	DDX_Control( pDX, IDC_EDIT_MOUSEY, m_MouseY );
}

BEGIN_MESSAGE_MAP(CTicketPriceSetterDlg, CDialogEx)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BUTTON_REX, &CTicketPriceSetterDlg::OnBnClickedButtonRex)
	ON_EN_CHANGE( IDC_EDIT_MOUSEX, &CTicketPriceSetterDlg::OnEnChangeEditMousex )
	ON_EN_CHANGE( IDC_EDIT_MOUSEY, &CTicketPriceSetterDlg::OnEnChangeEditMousey )
	ON_WM_TIMER()
END_MESSAGE_MAP()


// CTicketPriceSetterDlg message handlers
static const int kTimer1 = 1;

BOOL CTicketPriceSetterDlg::OnInitDialog()
{
	CDialogEx::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	// TODO: Add extra initialization here
	SetTimer( kTimer1, 100, nullptr );

	{
		auto x = AfxGetApp()->GetProfileInt( L"Mouse", L"X", 0 );
		std::wstringstream ss;
		ss << x;
		m_MouseX.SetWindowTextW( ss.str().c_str() );
	}

	{
		auto y = AfxGetApp()->GetProfileInt( L"Mouse", L"Y", 0 );
		std::wstringstream ss;
		ss << y;
		m_MouseY.SetWindowTextW( ss.str().c_str() );
	}

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
	auto x = AfxGetApp()->GetProfileInt( L"Mouse", L"X", 0 );
	auto y = AfxGetApp()->GetProfileInt( L"Mouse", L"Y", 0 );

	CWnd* ticketPricesWindow = FindWindow( NULL, TEXT( "Ticket Prices and Stadium Information" ) );
	if ( !ticketPricesWindow )
	{
		AfxMessageBox( TEXT( "Ticket prices did not open.\n" ), MB_OK | MB_ICONSTOP );
		return;
	}
	CWnd* recommendButton = ticketPricesWindow->GetDlgItem( 0x58d );
	if ( !recommendButton )
	{
		AfxMessageBox( TEXT( "No recommend button in ticket prices window?\n" ), MB_OK | MB_ICONSTOP );
		return;
	}
	CComboBox* ticketPriceTeamBox = static_cast<CComboBox*>( ticketPricesWindow->GetDlgItem( 0x58B ) );
	if ( !ticketPriceTeamBox )
	{
		AfxMessageBox( TEXT( "No team box in ticket price window?\n" ), MB_OK | MB_ICONSTOP );
		return;
	}

	// Need screen coordinates to figure out the mapping
	int screenX = GetSystemMetrics(SM_CXSCREEN);
	int screenY = GetSystemMetrics(SM_CYSCREEN);

	for (int teamIndex = 0; teamIndex < 32; ++teamIndex)
	{
		// Click change color
		DoMouseClick(x, y, screenX, screenY);

		// Change the color to the correct team
		CWnd* colorChooserWindow = FindWindow(NULL, TEXT("Color and Display Options Chooser"));
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

		ticketPriceTeamBox->SetCurSel( teamIndex );
		wParam = MAKEWPARAM( 1127, CBN_SELCHANGE );
		colorChooserWindow->SendMessage( WM_COMMAND, wParam, (LPARAM)( ticketPriceTeamBox->GetSafeHwnd() ) );
		Sleep( 250 );

		::SendMessage(recommendButton->GetSafeHwnd(), BM_CLICK, 0, 0);
		Sleep(250);
	}
}

void CTicketPriceSetterDlg::DoMouseClick(int x, int y, int screenX, int screenY)
{
	INPUT inputStruct;
	ZeroMemory(&inputStruct, sizeof(inputStruct));
	long clickX = (x * 65535) / screenX;
	long clickY = (y * 65535) / screenY;

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

void CTicketPriceSetterDlg::OnEnChangeEditMousex()
{
	CString mouseX;
	m_MouseX.GetWindowText( mouseX );
	auto start = mouseX.GetString();
	wchar_t* end;
	auto x = std::wcstol( start, &end, 10 );
	AfxGetApp()->WriteProfileInt( L"Mouse" , L"X" , x );
}


void CTicketPriceSetterDlg::OnEnChangeEditMousey()
{
	CString mouseY;
	m_MouseY.GetWindowText( mouseY );
	auto start = mouseY.GetString();
	wchar_t* end;
	auto y = std::wcstol( start, &end, 10 );
	AfxGetApp()->WriteProfileInt( L"Mouse", L"Y", y );
}


void CTicketPriceSetterDlg::OnTimer( UINT_PTR nIDEvent )
{
	POINT point;
	GetCursorPos( &point );

	std::wstringstream ss;
	ss << point.x << L", " << point.y;
	m_MousePosition.SetWindowTextW( ss.str().c_str() );

	CDialogEx::OnTimer( nIDEvent );
}
