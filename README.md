# Taskkiller
**Taskkiller** will find the Process with the supplied name by calling *tasklist* to get the PID and attempt to kill it using *taskkill*.
## Usage:
Taskkiller \<Process Name or part of it\> \<-c\>  
&emsp;If multiple Processes fit the supplied name, a list with possible matches is printed.  
&emsp;If -c is set, Taskkiller will ask you to confirm the killing of the Process.  
