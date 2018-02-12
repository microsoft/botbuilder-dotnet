for /d %%s in (*microsoft*) do call cleanup.cmd %%s
for /r %%s in (*.nuspec) do nuget pack %%s
