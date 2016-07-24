
// DraftRecorderDlg.h : header file
//

#pragma once
#include "afxwin.h"


// CDraftRecorderDlg dialog
class CDraftRecorderDlg : public CDialog
{
// Construction
public:
	CDraftRecorderDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_DRAFTRECORDER_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;

	bool DraftPlayer(int playerIndex);
	CListCtrl* m_scoutingReport;
	CWnd* m_draftPlayerWindow;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnBnClickedButtonDraftPlayers();
	CEdit m_playersEdit;
};
