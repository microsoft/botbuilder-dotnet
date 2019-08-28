@echo off

erase app.schema
dialogschema ../../libraries/**/*.schema ./**/*.schema -o app.schema 