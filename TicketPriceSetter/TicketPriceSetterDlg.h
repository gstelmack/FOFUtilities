
// TicketPriceSetterDlg.h : header file
//

#pragma once


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

	void DoMouseClick(int x, int y, CWnd* window, int screenX, int screenY);

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnBnClickedButtonRex();
};
