The whitelist system is build around two scripts:
- Whitelist Manager
- Synced Whitelist Manager

Both have multiple methods that you can use to:
- replace the whitelist
- add multiple users
- add a singular user
- remove a user

You can also get info back from them:
- the whitelisted username list
- the whitelisted username list formatted as a single string with one line per username

The normal Whitelist Manager is ideal for things a relatively static whitelist.
- best for things like String Loading
  - there is a script for that in the Extensions Folder

The Synced Whitelist Manager is ideal for a whitelist you want to dynamically update.
- only people who are whitelisted can change it
- not ideal for using things like a String Loaded whitelist
  - there are problems with this due to the whitelist check etc.
  
If you want to display the whitelist you can check out the whitelist name display script in the extension folder, or use the prefab.
