# Persistent-Icon-Changer
This tool allows you to change folder icons and display them even on different Windows machines, this is useful if you want to change
the way folders look on an external hard drive and see the same icons even when using it on a different computer.
There is also an option to restore the folder to the default icon.

The program works by saving an hidden copy of the desired icon inside the selected folder, then a desktop.ini file is created, pointing at
the icon inside the folder, so that it can be retrieved on every system.

Note that it may take a while for the icon to change in explorer, this has to do with the way Windows manages the icon cache, so try rebooting explorer.exe if the icon doesn't change.
