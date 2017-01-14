
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
	DDX_Control( pDX, IDC_EDIT_CONTROLS_X, m_ControlsX );
	DDX_Control( pDX, IDC_EDIT_CONTROLS_Y, m_ControlsY );
	DDX_Control( pDX, IDC_EDIT_OPTIONS_X, m_OptionsX );
	DDX_Control( pDX, IDC_EDIT_OPTIONS_Y, m_OptionsY );
	DDX_Control( pDX, IDC_EDIT_DISPLAY_OPTIONS_X, m_DisplayOptionsX );
	DDX_Control( pDX, IDC_EDIT_DISPLAY_OPTIONS_Y, m_DisplayOptionsY );
	DDX_Control( pDX, IDC_EDIT_ALMANAC_X, m_AlmanacX );
	DDX_Control( pDX, IDC_EDIT_ALMANAC_Y, m_AlmanacY );
	DDX_Control( pDX, IDC_EDIT_BLUE_BOOK_X, m_BlueBookX );
	DDX_Control( pDX, IDC_EDIT_BLUE_BOOK_Y, m_BlueBookY );
	DDX_Control( pDX, IDC_EDIT_STADIUM_X, m_StadiumX );
	DDX_Control( pDX, IDC_EDIT_STADIUM_Y, m_StadiumY );
}

BEGIN_MESSAGE_MAP(CTicketPriceSetterDlg, CDialogEx)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BUTTON_REX, &CTicketPriceSetterDlg::OnBnClickedButtonRex)
	ON_WM_TIMER()
	ON_EN_CHANGE( IDC_EDIT_CONTROLS_X, &CTicketPriceSetterDlg::OnEnChangeEditControlsX )
	ON_EN_CHANGE( IDC_EDIT_CONTROLS_Y, &CTicketPriceSetterDlg::OnEnChangeEditControlsY )
	ON_EN_CHANGE( IDC_EDIT_OPTIONS_X, &CTicketPriceSetterDlg::OnEnChangeEditOptionsX )
	ON_EN_CHANGE( IDC_EDIT_OPTIONS_Y, &CTicketPriceSetterDlg::OnEnChangeEditOptionsY )
	ON_EN_CHANGE( IDC_EDIT_DISPLAY_OPTIONS_X, &CTicketPriceSetterDlg::OnEnChangeEditDisplayOptionsX )
	ON_EN_CHANGE( IDC_EDIT_DISPLAY_OPTIONS_Y, &CTicketPriceSetterDlg::OnEnChangeEditDisplayOptionsY )
	ON_EN_CHANGE( IDC_EDIT_ALMANAC_X, &CTicketPriceSetterDlg::OnEnChangeEditAlmanacX )
	ON_EN_CHANGE( IDC_EDIT_ALMANAC_Y, &CTicketPriceSetterDlg::OnEnChangeEditAlmanacY )
	ON_EN_CHANGE( IDC_EDIT_BLUE_BOOK_X, &CTicketPriceSetterDlg::OnEnChangeEditBlueBookX )
	ON_EN_CHANGE( IDC_EDIT_BLUE_BOOK_Y, &CTicketPriceSetterDlg::OnEnChangeEditBlueBookY )
	ON_EN_CHANGE( IDC_EDIT_STADIUM_X, &CTicketPriceSetterDlg::OnEnChangeEditStadiumX )
	ON_EN_CHANGE( IDC_EDIT_STADIUM_Y, &CTicketPriceSetterDlg::OnEnChangeEditStadiumY )
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

	{
		auto x = AfxGetApp()->GetProfileInt( L"Controls", L"X", 0 );
		std::wstringstream ss;
		ss << x;
		m_ControlsX.SetWindowTextW( ss.str().c_str() );
	}

	{
		auto y = AfxGetApp()->GetProfileInt( L"Controls", L"Y", 0 );
		std::wstringstream ss;
		ss << y;
		m_ControlsY.SetWindowTextW( ss.str().c_str() );
	}
	{
		auto x = AfxGetApp()->GetProfileInt( L"Almanac", L"X", 0 );
		std::wstringstream ss;
		ss << x;
		m_AlmanacX.SetWindowTextW( ss.str().c_str() );
	}

	{
		auto y = AfxGetApp()->GetProfileInt( L"Almanac", L"Y", 0 );
		std::wstringstream ss;
		ss << y;
		m_AlmanacY.SetWindowTextW( ss.str().c_str() );
	}
	{
		auto x = AfxGetApp()->GetProfileInt( L"BlueBook", L"X", 0 );
		std::wstringstream ss;
		ss << x;
		m_BlueBookX.SetWindowTextW( ss.str().c_str() );
	}

	{
		auto y = AfxGetApp()->GetProfileInt( L"BlueBook", L"Y", 0 );
		std::wstringstream ss;
		ss << y;
		m_BlueBookY.SetWindowTextW( ss.str().c_str() );
	}
	{
		auto x = AfxGetApp()->GetProfileInt( L"DisplayOptions", L"X", 0 );
		std::wstringstream ss;
		ss << x;
		m_DisplayOptionsX.SetWindowTextW( ss.str().c_str() );
	}

	{
		auto y = AfxGetApp()->GetProfileInt( L"DisplayOptions", L"Y", 0 );
		std::wstringstream ss;
		ss << y;
		m_DisplayOptionsY.SetWindowTextW( ss.str().c_str() );
	}
	{
		auto x = AfxGetApp()->GetProfileInt( L"Stadium", L"X", 0 );
		std::wstringstream ss;
		ss << x;
		m_StadiumX.SetWindowTextW( ss.str().c_str() );
	}

	{
		auto y = AfxGetApp()->GetProfileInt( L"Stadium", L"Y", 0 );
		std::wstringstream ss;
		ss << y;
		m_StadiumY.SetWindowTextW( ss.str().c_str() );
	}
	{
		auto x = AfxGetApp()->GetProfileInt( L"Options", L"X", 0 );
		std::wstringstream ss;
		ss << x;
		m_OptionsX.SetWindowTextW( ss.str().c_str() );
	}

	{
		auto y = AfxGetApp()->GetProfileInt( L"Options", L"Y", 0 );
		std::wstringstream ss;
		ss << y;
		m_OptionsY.SetWindowTextW( ss.str().c_str() );
	}

	// TODO: Add extra initialization here
	SetTimer( kTimer1, 100, nullptr );

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
	auto almanacX = AfxGetApp()->GetProfileInt( L"Almanac", L"X", 0 );
	auto almanacY = AfxGetApp()->GetProfileInt( L"Almanac", L"Y", 0 );
	auto blueBookX = AfxGetApp()->GetProfileInt( L"BlueBook", L"X", 0 );
	auto blueBookY = AfxGetApp()->GetProfileInt( L"BlueBook", L"Y", 0 );
	auto controlsX = AfxGetApp()->GetProfileInt( L"Controls", L"X", 0 );
	auto controlsY = AfxGetApp()->GetProfileInt( L"Controls", L"Y", 0 );
	auto displayOptionsX = AfxGetApp()->GetProfileInt( L"DisplayOptions", L"X", 0 );
	auto displayOptionsY = AfxGetApp()->GetProfileInt( L"DisplayOptions", L"Y", 0 );
	auto optionsX = AfxGetApp()->GetProfileInt( L"Options", L"X", 0 );
	auto optionsY = AfxGetApp()->GetProfileInt( L"Options", L"Y", 0 );
	auto stadiumX = AfxGetApp()->GetProfileInt( L"Stadium", L"X", 0 );
	auto stadiumY = AfxGetApp()->GetProfileInt( L"Stadium", L"Y", 0 );

	// Need screen coordinates to figure out the mapping
	auto screenX = GetSystemMetrics(SM_CXSCREEN);
	auto screenY = GetSystemMetrics(SM_CYSCREEN);

	for (auto teamIndex = 0; teamIndex < 32; ++teamIndex)
	{
		// Hover mouse over controls
		HoverMouse( controlsX, controlsY, screenX, screenY );

		// Hover mouse over Options
		HoverMouse( optionsX, optionsY, screenX, screenY );

		// Click change color
		DoMouseClick( displayOptionsX, displayOptionsY, screenX, screenY);

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

		// Hover over almanac
		HoverMouse( almanacX, almanacY, screenX, screenY );

		// Hover over blue book
		HoverMouse( blueBookX, blueBookY, screenX, screenY );

		// Click stadium overview
		DoMouseClick( stadiumX, stadiumY, screenX, screenY );

		// Find and set ticket prices
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
		CWnd* submitButton = ticketPricesWindow->GetDlgItem( 0x532 );
		if ( !submitButton )
		{
			AfxMessageBox( TEXT( "No submit button in ticket prices window?\n" ), MB_OK | MB_ICONSTOP );
			return;
		}

		CComboBox* ticketPriceTeamBox = static_cast<CComboBox*>( ticketPricesWindow->GetDlgItem( 0x58B ) );
		if ( !ticketPriceTeamBox )
		{
			AfxMessageBox( TEXT( "No team box in ticket price window?\n" ), MB_OK | MB_ICONSTOP );
			return;
		}

		ticketPriceTeamBox->SetCurSel( teamIndex );
		wParam = MAKEWPARAM( 1127, CBN_SELCHANGE );
		ticketPricesWindow->SendMessage( WM_COMMAND, wParam, (LPARAM)( ticketPriceTeamBox->GetSafeHwnd() ) );
		Sleep( 250 );

		// Recommend prices
		::SendMessage( recommendButton->GetSafeHwnd(), BM_CLICK, 0, 0 );
		Sleep( 250 );

		// Submit prices
		::SendMessage( submitButton->GetSafeHwnd(), BM_CLICK, 0, 0 );
		Sleep( 250 );
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

void CTicketPriceSetterDlg::HoverMouse( int x, int y, int screenX, int screenY )
{
	INPUT inputStruct;
	ZeroMemory( &inputStruct, sizeof( inputStruct ) );
	long clickX = ( x * 65535 ) / screenX;
	long clickY = ( y * 65535 ) / screenY;

	inputStruct.mi.dx = clickX;
	inputStruct.mi.dy = clickY;

	inputStruct.mi.dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE;
	SendInput( 1, &inputStruct, sizeof( inputStruct ) );
	Sleep( 500 );
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


void CTicketPriceSetterDlg::OnEnChangeEditControlsX()
{
	CString pos;
	m_ControlsX.GetWindowText( pos );
	auto start = pos.GetString();
	wchar_t* end;
	auto posVal = std::wcstol( start, &end, 10 );
	AfxGetApp()->WriteProfileInt( L"Controls", L"X", posVal );
}


void CTicketPriceSetterDlg::OnEnChangeEditControlsY()
{
	CString pos;
	m_ControlsY.GetWindowText( pos );
	auto start = pos.GetString();
	wchar_t* end;
	auto posVal = std::wcstol( start, &end, 10 );
	AfxGetApp()->WriteProfileInt( L"Controls", L"Y", posVal );
}


void CTicketPriceSetterDlg::OnEnChangeEditOptionsX()
{
	CString pos;
	m_OptionsX.GetWindowText( pos );
	auto start = pos.GetString();
	wchar_t* end;
	auto posVal = std::wcstol( start, &end, 10 );
	AfxGetApp()->WriteProfileInt( L"Options", L"X", posVal );
}


void CTicketPriceSetterDlg::OnEnChangeEditOptionsY()
{
	CString pos;
	m_OptionsY.GetWindowText( pos );
	auto start = pos.GetString();
	wchar_t* end;
	auto posVal = std::wcstol( start, &end, 10 );
	AfxGetApp()->WriteProfileInt( L"Options", L"Y", posVal );
}


void CTicketPriceSetterDlg::OnEnChangeEditDisplayOptionsX()
{
	CString pos;
	m_DisplayOptionsX.GetWindowText( pos );
	auto start = pos.GetString();
	wchar_t* end;
	auto posVal = std::wcstol( start, &end, 10 );
	AfxGetApp()->WriteProfileInt( L"DisplayOptions", L"X", posVal );
}


void CTicketPriceSetterDlg::OnEnChangeEditDisplayOptionsY()
{
	CString pos;
	m_DisplayOptionsY.GetWindowText( pos );
	auto start = pos.GetString();
	wchar_t* end;
	auto posVal = std::wcstol( start, &end, 10 );
	AfxGetApp()->WriteProfileInt( L"DisplayOptions", L"Y", posVal );
}


void CTicketPriceSetterDlg::OnEnChangeEditAlmanacX()
{
	CString pos;
	m_AlmanacX.GetWindowText( pos );
	auto start = pos.GetString();
	wchar_t* end;
	auto posVal = std::wcstol( start, &end, 10 );
	AfxGetApp()->WriteProfileInt( L"Almanac", L"X", posVal );
}


void CTicketPriceSetterDlg::OnEnChangeEditAlmanacY()
{
	CString pos;
	m_AlmanacY.GetWindowText( pos );
	auto start = pos.GetString();
	wchar_t* end;
	auto posVal = std::wcstol( start, &end, 10 );
	AfxGetApp()->WriteProfileInt( L"Almanac", L"Y", posVal );
}


void CTicketPriceSetterDlg::OnEnChangeEditBlueBookX()
{
	CString pos;
	m_BlueBookX.GetWindowText( pos );
	auto start = pos.GetString();
	wchar_t* end;
	auto posVal = std::wcstol( start, &end, 10 );
	AfxGetApp()->WriteProfileInt( L"BlueBook", L"X", posVal );
}


void CTicketPriceSetterDlg::OnEnChangeEditBlueBookY()
{
	CString pos;
	m_BlueBookY.GetWindowText( pos );
	auto start = pos.GetString();
	wchar_t* end;
	auto posVal = std::wcstol( start, &end, 10 );
	AfxGetApp()->WriteProfileInt( L"BlueBook", L"Y", posVal );
}


void CTicketPriceSetterDlg::OnEnChangeEditStadiumX()
{
	CString pos;
	m_StadiumX.GetWindowText( pos );
	auto start = pos.GetString();
	wchar_t* end;
	auto posVal = std::wcstol( start, &end, 10 );
	AfxGetApp()->WriteProfileInt( L"Stadium", L"X", posVal );
}


void CTicketPriceSetterDlg::OnEnChangeEditStadiumY()
{
	CString pos;
	m_StadiumY.GetWindowText( pos );
	auto start = pos.GetString();
	wchar_t* end;
	auto posVal = std::wcstol( start, &end, 10 );
	AfxGetApp()->WriteProfileInt( L"Stadium", L"Y", posVal );
}
