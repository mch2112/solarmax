!include "MUI2.nsh"
!include "UAC.nsh"

Name "SolarMax"
Caption "SolarMax Setup"
OutFile "SolarMaxInstaller.exe"

InstallDir $PROGRAMFILES\SolarMax

!define VERSION '1.0.5.0'
!define VERSION_DISPLAY '1.0.5'

; Registry key to check for directory (so if you install again, it will overwrite the old one automatically)
InstallDirRegKey HKLM "Software\SolarMax" "Install_Dir"

RequestExecutionLevel user

!define MUI_ICON "solarmax.ico"
!define MUI_UNICON "solarmax.ico"
!define MUI_ABORTWARNING
!define MUI_UNWELCOMEFINISHPAGE_BITMAP "panel.bmp"
!define MUI_HEADERIMAGE
!define MUI_WELCOMEFINISHPAGE_BITMAP "panel.bmp"
!define MUI_HEADERIMAGE_BITMAP "${NSISDIR}\Contrib\Graphics\Header\win.bmp"
!define MUI_HEADERIMAGE_UNBITMAP "${NSISDIR}\Contrib\Graphics\Header\win.bmp"
!define MUI_FINISHPAGE_RUN "$INSTDIR\SolarMaxStart.exe"
!define MUI_WELCOMEPAGE_TITLE "Welcome to the SolarMax Setup Wizard"
!define MUI_WELCOMEPAGE_TEXT "This wizard will guide you through the installation of SolarMax version ${VERSION_DISPLAY}."
!define MUI_FINISHPAGE_LINK "Visit www.solar-max.org for updates and support"
!define MUI_FINISHPAGE_LINK_LOCATION "http://www.solar-max.org/"
!define MUI_FINISHPAGE_NOREBOOTSUPPORT
!define MUI_FINISHPAGE_TEXT_LARGE
!define MUI_FINISHPAGE_TEXT "Enjoy using SolarMax! Please visit www.solar-max.org for updates and more information, or to make a donation!"

VIProductVersion ${VERSION}
VIAddVersionKey "ProductName" "SolarMax"
VIAddVersionKey "LegalCopyright" "(c) 2011 Matthew Hamilton"
VIAddVersionKey "FileDescription" "SolarMax Installer ${VERSION}"
VIAddVersionKey "FileVersion" ${VERSION}
VIAddVersionKey "InternalName" "Solar-Max"
VIAddVersionKey "Comments" "SolarMax is a simulation of the solar system."
VIAddVersionKey "CompanyName" "Matthew Hamilton"
VIAddVersionKey "ProductVersion" ${Version}
VIAddVersionKey "LegalTrademarks" "SolarMax is a Trademark of Matthew Hamilton."
VIAddVersionKey "OriginalFilename" "SolarMax.exe"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "license.txt"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
  
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"

BrandingText "SolarMax Installer"
  
;--------------------------------
; The stuff to install

Section "SolarMax (required)"

  SectionIn RO
  
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  File "SolarMax.exe"
  File "SolarMaxStart.exe"
  File "colors_template.txt"
  File "preferences_template.txt"
  
  SetOutPath $INSTDIR\celestial_data
  
  Delete "$INSTDIR\celestial_data\ephemeris*.*"
  
  File /x "ephemeris_snapshot*.txt" "celestial_data\ephemeris*.txt" 
  File "celestial_data\constant_data.txt"
  File "celestial_data\constellation_boundaries.txt"
  File "celestial_data\constellations.txt"
  File "celestial_data\stars.txt"
  
  Delete "$INSTDIR\celestial_data\ephemeris_snapshot.txt"
  Delete "$INSTDIR\celestial_data\colors.txt"
  Delete "$INSTDIR\celestial_data\preferences.txt"

  AccessControl::GrantOnFile "$INSTDIR\celestial_data" "(BU)" "FullAccess"

  ; Write the installation path into the registry
  WriteRegStr HKLM SOFTWARE\SolarMax "Install_Dir" "$INSTDIR"

  CreateShortCut "$SMPROGRAMS\SolarMax.lnk" "$INSTDIR\SolarMaxStart.exe";
  CreateShortCut "$DESKTOP\SolarMax.lnk" "$INSTDIR\SolarMaxStart.exe";
  CreateShortCut "$QUICKLAUNCH\SolarMax.lnk" "$INSTDIR\SolarMaxStart.exe"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SolarMax" "DisplayName" "SolarMax - Solar System Simulator"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SolarMax" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SolarMax" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SolarMax" "NoRepair" 1
  WriteUninstaller "uninstall.exe"
  
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ImageMaker" \
                 "DisplayName" "Image Maker -- super software from Great Northern"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ImageMaker" \
                 "UninstallString" "$\"$INSTDIR\uninstall.exe$\""
  
SectionEnd

;--------------------------------
; Uninstaller

Section "Uninstall"
  
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SolarMax"
  DeleteRegKey HKLM SOFTWARE\SolarMax

  Delete $INSTDIR\SolarMax.exe
  Delete $INSTDIR\SolarMaxStart.exe
  Delete $INSTDIR\uninstall.exe
  Delete "$INSTDIR\celestial_data\*.*"
  Delete "$SMPROGRAMS\SolarMax\*.*"

  RMDir "$SMPROGRAMS\SolarMax"
  RMDir "$INSTDIR\celestial_data"
  RMDir "$INSTDIR"

SectionEnd


Function .onInit
	uac_tryagain:
	!insertmacro UAC_RunElevated
	#MessageBox mb_TopMost "0=$0 1=$1 2=$2 3=$3"
	${Switch} $0
	${Case} 0
		${IfThen} $1 = 1 ${|} Quit ${|} ;we are the outer process, the inner process has done its work, we are done
		${IfThen} $3 <> 0 ${|} ${Break} ${|} ;we are admin, let the show go on
		${If} $1 = 3 ;RunAs completed successfully, but with a non-admin user
			MessageBox mb_IconExclamation|mb_TopMost|mb_SetForeground "This installer requires admin access, please try again" /SD IDNO IDOK uac_tryagain IDNO 0
		${EndIf}
		;fall-through and die
	${Case} 1223
		MessageBox mb_IconStop|mb_TopMost|mb_SetForeground "This installer requires admin privileges."
		Quit
	${Case} 1062
		MessageBox mb_IconStop|mb_TopMost|mb_SetForeground "Logon service not running, exiting."
		Quit
	${Default}
		MessageBox mb_IconStop|mb_TopMost|mb_SetForeground "Unable to elevate , error $0"
		Quit
	${EndSwitch}
FunctionEnd

Function un.onInit
	uac_tryagain:
	!insertmacro UAC_RunElevated
	#MessageBox mb_TopMost "0=$0 1=$1 2=$2 3=$3"
	${Switch} $0
	${Case} 0
		${IfThen} $1 = 1 ${|} Quit ${|} ;we are the outer process, the inner process has done its work, we are done
		${IfThen} $3 <> 0 ${|} ${Break} ${|} ;we are admin, let the show go on
		${If} $1 = 3 ;RunAs completed successfully, but with a non-admin user
			MessageBox mb_IconExclamation|mb_TopMost|mb_SetForeground "This installer requires admin access, please try again" /SD IDNO IDOK uac_tryagain IDNO 0
		${EndIf}
		;fall-through and die
	${Case} 1223
		MessageBox mb_IconStop|mb_TopMost|mb_SetForeground "This installer requires admin privileges."
		Quit
	${Case} 1062
		MessageBox mb_IconStop|mb_TopMost|mb_SetForeground "Logon service not running, exiting."
		Quit
	${Default}
		MessageBox mb_IconStop|mb_TopMost|mb_SetForeground "Unable to elevate , error $0"
		Quit
	${EndSwitch}
FunctionEnd
