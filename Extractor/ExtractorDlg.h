// ExtractorDlg.h : header file
//

#pragma once
#include "afxwin.h"


// CExtractorDlg dialog
class CExtractorDlg : public CDialog
{
// Construction
public:
	CExtractorDlg(CWnd* pParent = NULL);	// standard constructor
	~CExtractorDlg();

// Dialog Data
	enum { IDD = IDD_EXTRACTOR_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()

	afx_msg void OnBnClickedExtractButton();
	CStatic mCurrentPlayerStatic;

	void GrabPowerRatings(CWnd* ratingsWindow);
	void GrabWeeklySchedule(CWnd* scheduleWindow);

	void ProcessScoutReportWindow(CWnd* reportWindow);
	void ProcessPlayerReportWindow(CWnd* reportWindow,bool appendToFile);
	void ProcessStaffReportWindow(CWnd* reportWindow);
	void ProcessRosterWindow(CWnd* reportWindow);
	void ProcessWeeklySchedule(CWnd* reportWindow);
	CButton mProcessFreeAgentsCheck;
    CButton mOutputFilenameButton;

public:
    afx_msg void OnBnClickedButtonFilename();
};
