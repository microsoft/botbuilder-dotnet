for /d %%s in (*microsoft*) do rd /s /q %%s\package 
for /d %%s in (*microsoft*) do rd /s /q %%s\_rels 
erase /s /q [Content_Types].xml
for /r %%s in (*.nuspec) do nuget pack %%s
for /d %%s in (*microsoft*) do rd /s /q %%s
