chcp 65001
echo Windows Registry Editor Version 5.00 >> InstalujProtokol.reg
echo.  >> InstalujProtokol.reg
echo [HKEY_CLASSES_ROOT\sharpwars] >> InstalujProtokol.reg
echo @="URL: SharpWars Game Protocol 4 Uber Pro Hax" >> InstalujProtokol.reg
echo "URL Protocol"="" >> InstalujProtokol.reg
echo.  >> InstalujProtokol.reg
echo [HKEY_CLASSES_ROOT\sharpwars\shell] >> InstalujProtokol.reg
echo.  >> InstalujProtokol.reg
echo [HKEY_CLASSES_ROOT\sharpwars\shell\open] >> InstalujProtokol.reg
echo.  >> InstalujProtokol.reg
echo [HKEY_CLASSES_ROOT\sharpwars\shell\open\command] >> InstalujProtokol.reg
echo @="\"%CD:\=\\%\\SharpWars.exe\" \"%%1\"" >> InstalujProtokol.reg
echo.  >> InstalujProtokol.reg