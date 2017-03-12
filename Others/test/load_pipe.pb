
ReadFile(1,"test.xps")
Str.s = Space(Lof(1))

res = ReadData(1,@str,Lof(1))

pipe = CreateNamedPipe_("\\.\pipe\RN215", #PIPE_ACCESS_OUTBOUND, #PIPE_TYPE_BYTE|#PIPE_WAIT, 1, 4096, 0, 250, #Null)

fConnected = ConnectNamedPipe_(pipe, #Null)  

remaining = Lof(1)
offset = 0 
While (remaining > 0)   
  numWritten = 0
  If remaining > 4096
    WriteFile_(pipe, @Str + offset, 4096, @numWritten, #Null)
  Else
    WriteFile_(pipe, @Str + offset, remaining, @numWritten, #Null)  
  EndIf  
  
  remaining - numWritten
  offset + numWritten
Wend

CloseHandle_(pipe)
CloseFile(1)

; IDE Options = PureBasic 5.00 Beta 3 (Windows - x86)
; CursorPosition = 1
; EnableXP
; EnableOnError
; Executable = xpsview_express\xpsview\bin\Release\AnyCPU\test.exe