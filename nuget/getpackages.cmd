pushd ..\library
for /r %%s in (*.nupkg) do xcopy /d %%s ..\nuget
popd


