;--------------------------------
; Defines

!define VERSION "1.2.0"

;--------------------------------
; General

; Name/File
Name "mTiler ${VERSION}"
OutFile "mTiler_${VERSION}_setup.exe"

SetCompressor /SOLID LZMA

; Default install Dir
InstallDir "C:\Program Files\mTiler"

; We need amin rights
RequestExecutionLevel admin

;--------------------------------
; Modern UI

!include "MUI2.nsh"

;--------------------------------
; Pages

!insertmacro MUI_PAGE_LICENSE "..\LICENSE"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES

;--------------------------------
; Installer

Section "install"

    SetOutPath "$INSTDIR"
    SetRegView 64

        ;; Winbuntu components
        DetailPrint "Installing mTiler..."
        File "..\mTiler\bin\Release\Microsoft.WindowsAPICodePack.dll"
        File "..\mTiler\bin\Release\Microsoft.WindowsAPICodePack.Shell.dll"
        File "..\mTiler\bin\Release\mTiler.exe"
        File "..\mtiler.ico"

        ;; Start menu entry
        createDirectory "$SMPROGRAMS\mTiler"
        createShortcut "$SMPROGRAMS\mTiler\mTiler.lnk" "$INSTDIR\mTiler.exe" "" "$INSTDIR\mtiler.ico"

        ;; Uninstaller
        DetailPrint "Creating Uninstaller..."
        WriteUninstaller "$INSTDIR\uninstall.exe"

        ;; Uninstaller Registry
        WriteRegExpandStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\mTiler" "UninstallString" "$INSTDIR\uninstall.exe"
        WriteRegExpandStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\mTiler" "InstallLocation" "$INSTDIR"
        WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\mTiler" "DisplayName" "mTiler ${VERSION}"
        WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\mTiler" "DisplayVersion" "${VERSION}"
        WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\mTiler" "NoModify" "1"
        WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\mTiler" "NoRepair" "1"

SectionEnd

;--------------------------------
; Uninstaller

Section "Uninstall"

    ;; Delete the Files
    RMDir /r "$INSTDIR\*.*"
    RMDir "$INSTDIR"

    ;; Remove Uninstaller
    DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\mTiler"

SectionEnd
