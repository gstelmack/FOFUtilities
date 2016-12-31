
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

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnBnClickedButtonRex();
	afx_msg void OnEnChangeEditMousex();
	afx_msg void OnEnChangeEditMousey();
protected:
	CStatic m_MousePosition;
	CEdit m_MouseX;
	CEdit m_MouseY;
public:
	afx_msg void OnTimer( UINT_PTR nIDEvent );
};
