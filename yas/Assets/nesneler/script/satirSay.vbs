Const ForReading = 1
dim baslangic 
dim toplam
dim sayisi
on error resume next  
baslangic  = Inputbox("Folder Name","Choose","D:\Kur\projelerim\unity\yazSurvival2017\yas\Assets\nesneler\script")
uzanti     = Inputbox("File Extension","Choose","cs")
tekrarli   = Msgbox("Include SubFolders", vbYesNo)
'bir hata meydana gelirse ekrana mesaj çýkmaz
   function tekrar (ByVal p)
    dim objFSO
    dim objFile
    dim s
    dim dizin
    dim altdizin 
    dim dosya
    Set objFSO = CreateObject("Scripting.FileSystemObject")
    set dizin = objFSO.GetFolder(p)
    	
    for each dosya in dizin.Files	
     if ucase(Mid(dosya.name, InStrRev(dosya.name, ".") + 1))=ucase(uzanti) and dosya.name<>"" then
      sayisi = sayisi + 1   
      
      Set objTextFile = objFSO.OpenTextFile(dizin & "\" & dosya.name, ForReading)   
      objTextFile.ReadAll
      toplam = toplam + objTextFile.Line  
     end if  

    next
     if tekrarli=vbYes then 
       for each altdizin in dizin.subfolders
          s=s & tekrar (altdizin)
       next
     end if 
   
     tekrar = s  'sonuc döner
   end function
tekrar(baslangic)
Wscript.Echo "Number of lines: " & toplam & " in " & sayisi 