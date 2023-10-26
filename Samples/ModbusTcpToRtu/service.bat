sc delete ModbusTcpToRtu 
sc create ModbusTcpToRtu "binPath=%~dp0ModbusTcpToRtu.exe" start= delayed-auto
sc start ModbusTcpToRtu
pause