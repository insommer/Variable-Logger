# Variable-Logger
A Cicero server that creates text files listing variables and their values for each run of the experiment.
Variables are saved in the format of a standard configuration file, where each line contains:
[variable] = [value]

Originally written by Eason Shen, based on the ExampleServer code in Cicero.

# Installation
To compile, first update the reference to DataStructures. 
To avoid redundant dlls, we have the reference pointing to the DataStructures.dll instance in the folder containing the copy of Cicero that we run. 
This also ensures compatibility.
