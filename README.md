# Valheim AutoStack Fix
Valheim plugin to fix item loss bug in multiplayer when many items on ground are auto stacked.

## What?
Valheim periodically counts the number of items lying on the ground. If this number is ≥ 100, nearby item stacks are merged with each other wherever possible.

In multiplayer, clients can become desynced and see items with a much lower value than the server is aware of (eg. `Wood x 4` on the client, while the server shows `Wood x 50`). On pickup, the "invisible" items are deleted; this aims to fix that.

## Changelog
* 0.0.1
  * First release.
