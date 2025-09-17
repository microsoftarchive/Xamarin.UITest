tell application "System Events"
	set terminalRunning to (count (processes whose name is "Terminal")) > 0
end tell

tell application "Terminal"

	if not terminalRunning then
		set myTab to do script "###START-REPL-SCRIPT###" in front window
	else
		set myTab to do script "###START-REPL-SCRIPT###"
	end if

	activate
	
	delay 3
	repeat until (not (exists myTab)) or (busy of myTab is false)
		delay 0.3
	end repeat
	
	if (exists myTab) then
		close (first window whose selected tab is myTab) saving no
	end if
	
	if (count of windows) is 0 then
		quit
	end if
	
end tell