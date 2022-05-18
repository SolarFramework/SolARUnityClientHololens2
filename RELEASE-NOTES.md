# Release Notes

## v0.5.0
* Add reset button
* Send a "stop" request to ARCloud services when application closes (if in "start" mode)
## v0.4.0
* Improve hologram alignment by applying the offset transfom between sensor camera and the eyes location.
* Use a coroutine to start sensors, fetch and send frames + control sending rate
* Add a marker-sized cube to debug registration accuracy

## v0.3.0

* Add JPEG compression option

## v0.2.0

* Add ability to select pipeline mode from relocation AND mapping to mapping only

## v0.1.0

* First release. This plugin provides a prefab which can be used to access SolAR relocalization and mapping in the cloud from a Unity application. 