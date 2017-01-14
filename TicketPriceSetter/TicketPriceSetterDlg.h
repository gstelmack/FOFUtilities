
// TicketPriceSetterDlg.h : header file
//

#pragma once
#include "afxwin.h"


// CTicketPriceSetterDlg dialog
class CTicketPriceSetterDlg : public CDialogEx
{
// Construction
public:
	CTicketPriceSetterDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_TICKETPRICESETTER_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;

	void DoMouseClick(int x, int y, int screenX, int screenY);
	void HoverMouse( int x, int y, int screenX, int screenY );

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()

	CStatic m_MousePosition;
	CEdit m_ControlsX;
	CEdit m_ControlsY;
	CEdit m_OptionsX;
	CEdit m_OptionsY;
	CEdit m_DisplayOptionsX;
	CEdit m_DisplayOptionsY;
	CEdit m_AlmanacX;
	CEdit m_AlmanacY;
	CEdit m_BlueBookX;
	CEdit m_BlueBookY;
	CEdit m_StadiumX;
	CEdit m_StadiumY;

public:
	afx_msg void OnBnClickedButtonRex();
	afx_msg void OnTimer( UINT_PTR nIDEvent );
	afx_msg void OnEnChangeEditControlsX();
	afx_msg void OnEnChangeEditControlsY();
	afx_msg void OnEnChangeEditOptionsX();
	afx_msg void OnEnChangeEditOptionsY();
	afx_msg void OnEnChangeEditDisplayOptionsX();
	afx_msg void OnEnChangeEditDisplayOptionsY();
	afx_msg void OnEnChangeEditAlmanacX();
	afx_msg void OnEnChangeEditAlmanacY();
	afx_msg void OnEnChangeEditBlueBookX();
	afx_msg void OnEnChangeEditBlueBookY();
	afx_msg void OnEnChangeEditStadiumX();
	afx_msg void OnEnChangeEditStadiumY();
};
