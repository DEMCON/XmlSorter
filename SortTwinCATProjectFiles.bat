REM Sorting Beckhoff TwinCAT project-files:
REM ---------------------------------------
REM Make sure the XmlSorter.exe is in the same dir as this batch-file.
REM Fill in the correct solution- or project-folder from your TwinCAT project.
REM The project-files noted here can be sorted.
REM Make sure the 'Device' nodes are NOT sorted, as show below.
REM (Demcon - Ruud Jeurissen - 11-04-2019)

XmlSorter.exe "<solution_folder>" "tsproj;xti;plcproj" "Device"
