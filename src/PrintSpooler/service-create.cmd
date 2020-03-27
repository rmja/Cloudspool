:: Create the print spooler spooler service (run this command as administrator)
:: sc create CloudspoolPrintSpooler BinPath="C:\Cloudspool\PrintSpooler\PrintSpooler.exe" Start=auto DisplayName="Cloudspool PrintSpooler"
  sc create CloudspoolPrintSpooler BinPath="C:\Source\Cloudspool2\src\PrintSpooler\bin\Release\netcoreapp3.1\win10-x64\publish\PrintSpooler.exe" Start=auto DisplayName="Cloudspool PrintSpooler"
sc start CloudspoolPrintSpooler
:: Example commands
:: sc start CloudspoolPrintSpooler
:: sc stop CloudspoolPrintSpooler
:: sc delete CloudspoolPrintSpooler