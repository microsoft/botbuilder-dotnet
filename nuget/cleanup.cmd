@if "%1" == "" goto end
rd /s /q %1\package 
rd /s /q %1\_rels 
erase /q %1\[Content_Types].xml
:end
