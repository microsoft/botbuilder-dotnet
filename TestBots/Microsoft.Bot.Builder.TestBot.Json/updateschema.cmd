@echo off

erase testbot.schema
bf dialog:merge ../../libraries/**/*.schema ./**/*.schema -o testbot.schema