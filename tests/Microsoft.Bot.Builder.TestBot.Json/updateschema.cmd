@echo off

erase app.schema
bf dialog:merge ../../libraries/**/*.schema ./**/*.schema -o app.schema 