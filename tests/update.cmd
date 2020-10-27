@echo Updating test schema files.
cd ..
bf dialog:merge libraries/**/*.schema libraries/**/*.uischema tests/**/*.schema -o tests/tests.schema --verbose
cd tests
