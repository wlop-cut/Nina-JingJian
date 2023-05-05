# Version 1.10

## Features

### Hardware
 - **QHY filter wheels** that are integrated (A-series cameras) or are connected to the camera using the 4-pin cable (CFW1/2/3 filter wheels) are now natively supported. This allows the native QHY camera driver to be used with these cameras and configurations.
 - **Alnitak Flat Device support** 
    - connect to and control Alnitak flat devices from within the application
    - flip-flat devices will automatically open and close within a sequence
    - option to close flip-flats and dust covers at the end of the sequence
 - **Omegon Pro Cameras** Added SDK for omegon pro cameras (untested)
 - **Lacerta MGEN Superguider integration**
    - Full control of the MGEN by mirroring the controller fully into N.I.N.A.'s User Interface
    - Automatic power on when connecting
    - Starting of guiding on sequence start
    - On start of guiding initiating a calibration and auto select star when required
    - Display star drift during guiding in a chart (currently only measured in pixels)
    - Dithering during sequencing
 - **Canon Cameras** Updated support for recently-released Canon DSLR and mirrorless cameras (EOS M6 Mark II, EOS 90D, EOS M200)
 - **Pegasus Astro Ultimate Powerbox V2 support**
   - connect and control the Ultimate Powerbox V2 from within the application
   - monitor input voltage and power consumption
   - turn power ports and usb ports on and off
   - set the voltage of the variable power port
   - control the dew heater ports, including setting the Auto-Dew feature
   - support for using the powerbox sensors as a weather device
   - support for using the stepper motor driver as a focuser
 - **Pegasus Astro Flatmaster support**
   - connect and control the Pegasus Astro Flatmaster from within the application
 - **Artesky USB Flat Box support**

## Localization
- To be able to support many languages N.I.N.A. is now available on Crowdin. This powerful online translation management tool will allow users to easily contribute to the localization.
- For more details visit https://nina.crowdin.com/ and feel free to participate in the ongoing effort to provide multiple languages for N.I.N.A.!

## Improvements
- New rows added to a sequence will now default to the values from the previous row
- Upgraded Atik driver to use latest Atik SDK and drivers
- Added a bit scaling options for Altair and ToupTek cameras to bit shift the raw data to 16 bits to be compatible with other capture software
- FreeImage library upgrade to 3.18
- Plate solvers will now receive unstretched FITS images to solve the image for a faster and even better solve result
- The autofocus crop ratio has been changed to Inner Crop Ratio, and an additional Outer Crop Ratio has been added. This lets users define centered ROI, or a centered "square doughnut" which will be used by star detection, thus avoiding stars in the center and at the edges of the FOV
- HFR calculation is now computed using the mean background surrounding the star, rather than the image mean
- HFR calculation has been enhanced to provide more accurate results, especially for imaging systems with central obstructions
- Autofocus trend lines are now using a weighted fit based on HFR standard dev in each image rather than an unweighted fit. This provides much better slopes and final focus point.
- Autofocus has been enhanced to support multiple curve fitting methodologies:
	- Parabolic fitting, weighted by standard dev
	- Hyperbolic fitting, weighted by standard dev
	- Comnbination of parabolic or hyperbolic fitting with trend lines (average of fitting minimum and trend line intersection is then used)
- Added ability to keep guiding during autofocus
- The autofocus routine has been changed so that it doesn't attempt to measure the focus twice for the same point
- The focuser temperature compensation feature is now turned off before an auto-focus session, and turned back on afterwards
- The NASA Sky Survey images now are automatically adjusted for brightness and contrast, depending on each image characteristic
- An autofocus filter can now be set if "Use filter offsets" is set to true. When defined, the autofocus routine will use the autofocus filter instead of the current imaging filter. Initial baseline HFR, and final HFR (used to determine whether the autofocus run was successful) will still use the main imaging filter.
- A contrast detection autofocus routine has been added. Instead of analyzing stars to determine point of best focus, the routine analyzes overall contrast of the image with various contrast detection methods. A Gaussian fit is then performed on the obtained focus points - this process can use shorter exposure times than star HFR and produce results faster
- QHY native camera driver now retrieves only the non-overscan area of the sensor
- QHY native camera driver now supports selecting readout modes with QHY600 class cameras
- QHY native camera driver now optimizes image file size based on the actual image dimensions used, resulting in generally smaller image file sizes
- Added a $$DATEMINUS12$$ placeholder to the file save pattern that shifts the current date 12 hours into the past. This allows for all images of a night to be saved into the same date folder.
- Added more manual focus targets for certain regions of the southern hemisphere sky.
- Can now configure a custom Astrometry.net API URL
- Improved status and logging for Astrometry.net plate solve jobs
- Updated Nikon SDK to latest available version (2019-10-20)
- Zooming at high magnification inside the image control will not smear the pixels anymore but show sharp pixel edges instead
- Added a reset button to sky atlas to reset all filters
- Rotator for sequence centering will now rotate to nearest orientation even if image is upside down. This is not relevant for framing after a star alignment anyways.
- Reworked Planetarium interfacing to be more robust
- Take current view center from Stellarium when no target is selected during coordinate import
- Logs older than 30 days are automatically cleaned up at application start
- Autofocus will now only show the selected fitting method 
- After running an autofocus run a new json file will be written to %localappdata%\nina\autofocus containing all datapoints to retrace what was measured at a later point in time
- Reintroduce a "No Guider" option under Equipment -> Guiding so that unguided setups do not suffer through unwanted PHD2 executions or unnecessary errors when the "Connect All Devices" button is pressed.
- Platesolve tab inside Imaging Tab will now retain its settings (Sync, Reslew, Repeat)
- Added the option to retreieve the coordinates of the center of the sky chart instead of the selected object when using TheSkyX as the planetarium
- Reworked the platesolving code base completely for a more clean way of oprations. 
- Introduced Gain and Binning settings to use for automated plate solves
- Removed "Sync" and "Repeat until" options from Imagine->PlateSolve tool window. These will be always on when "Reslew to target" is enabled
- Improved automated plate solve pop up window with additonaly information
- Added ability to record flat exposure times with the flat wizard, to be used in a sequence later
- Added estimated target start and end times to sequence view - useful for multi-target sequence sets
- XISF files may be created with optional compression or shuffled compression of the image data using LZ4, LZ4-HC, or ZLib (deflate) lossless compression algorithms. Decompression of compressed XISF files is also supported
- XISF files may be created with optional embedded checksums of the image data using SHA1, SHA-256, or SHA-512 hashing algorithms. Checksums are verified when opening a XISF file which has one
- Image history graph now displays autofocus indicators with details of the previous position and the newly calculated position as well as the focuser temperature at that point in time
- Added button to Telescope control window to set the current position of the mount to be its park position. This button is available only if the connected mount's ASCOM driver supports this action (CanSetPark = true)
- Optimal Exposure Calculator is moved out of the statistics window into its own tool window for improved user experience
- The pre-sequence checklist has been enhanced to check whether the telescope is parked - if so, and user validates, NINA will unpark the telescope before proceeding
- A new option is available to prevent sending any sync signal to the mount. The centering logic will then use offset coordinate calculation instead. This new logic will also be called when a sync fails.
- The Autofocus after HFR change has been enhanced to not be triggered by a single frame affected by adverse conditions such as wind
- Improved resiliency of StartGuider when PHD2 is unable to detect a guide star when guiding is started due to temporary clouds. An auto retry mechanism has been added to allow PHD2 to try selecting a new guide star after a configurable timeout.
- Added profile name to the title bar
- Camera snapshot control in imaging tab will now save the values for the next session
- Added `READOUTM` keyword to FITS and XISF files
- Added `$$SQM$$` file name pattern to allow the current reading from an attached sky quality meter to be used in file or folder names
- Added `$$READOUTMODE$$` file name pattern to enable the a camera's readout mode to be used in file or folder names
- Added `BAYERPAT`, `XBAYEROFF`, and `YBAYEROFF` keyword support in FITS and XISF files for color cameras. A menu under Options > Equipment > Camera allows the user to override the driver-specified Bayer pattern with a custom one
- Meridian Flip now has an option to do an auto focus after a flip during sequencing
- When the camera is not giving a ready signal for image download after exposuretime + 15 seconds the exposure will be canceled and skipped
- Guider graph pauses during dithering to not show guide pulses during dithering and just during "real" guiding
- Guider graph settings are now stored in profile and reloaded when application is started again
- Guider graph now displays a triangle as indicator when a dither action was happening
- NINA now uses the equatorial system reported by the mount driver and the setting under Options > General > Astrometry is removed. Mount drivers that report an "other" epoch will cause NINA to default to J2000

## Bug fixes
- Guiding was improperly stopped when performing AF at start of sequence, even if DisableGuiding option was false
- QHY native camera driver will wait the full amount of time required before downloading an image from the camera
- QHY native camera driver now has a more reliable way to determine sensor bitness with CCD models
- The font size of panel IDs in Framing Assistant has been increased to a legible level
- The $$FRAMENR$$ placeholder is now saving the number as 4 digits
- Framing - Rectangle panel number is now scaled based on rectangle size.
- When loading an image from a file into framing the rectangle was not calculated correctly
- Added an empty entry for Constellation Filter in Sky Atlas to unselect it
- Image File Pattern Values will now remove leading and trailing white spaces to prevent invalid file path
- In case illegal characters for a filename are inside some file patterns, they will now get replaced to still being able to save
- Canon camera support has been improved and many common errors have been fixed
- Autofocus after number of exposures now correctly triggers after the expected amount of exposures.
- ZWO cameras now properly handle odd bin dimensions (eg; 3x3)
- Fix FLI cameras sitting idle for the length of the exposure time before actually initiating the exposure
- Check and wait when using side of pier on meridian flip if the mount does the flip automatically. (for GS Server Driver)
- Binned exposures now finish on QHY cameras that have overscan areas
- Gain and Offset are not saved with decimal places anymore
- XPIXSZ and YPIXSZ now account correctly for binning when available
- Autofocus after HFR change properly resets reference index on sequence start, target change, and autofocus caused by other criteria
- Sequence file names are now saved with valid file name characters. Invalid characters are replaced automatically with a hyphen (-)
- Framing and Sequence Coordinates now allow for a "-0" to be entered in declination

## Special Thanks
The N.I.N.A. team would like to sincerely thank:
- The staff at [Teleskop Austria](https://teleskop-austria.com/) for providing an MGEN Unit as well as a detailed communication protocol document for implementing the MGEN into N.I.N.A. and being able to test it thoroughly.

These items helped a lot during development and testing.  
Thank you for your support!

## Included Camera SDK Versions:
- Altair: 43.15988.2019.1124
- Atik: 1.3.0.4
- Canon: 13.11.10
- FLI: 1.104.0.0
- Nikon: 1.3.1.3001
- Omegon: 39.15325.2019.810
- QHY: 20.2.3.1
- ToupTek: 30.13342.2018.1121
- ZWO: 1.14.11.19

# Version 1.9

## Features

### Camera Control
- Native support for Finger Lakes Instrumentation ProLine/MicroLine cameras and color filter wheels

### Switch Hubs
- A new equipment type "Switch" has been added which can connect to an AscomSwitch or to a PrimaLuceLab EAGLE PC

### Dither without guiding
- A new Direct Guider has been added, which doesn't require a guide camera. It can only perform random dithers. It connects directly to the telescope, and will if required perform a dither via a Pulse Guide of a user-provided duration in a random direction.
- Enhanced direct guider will accept decimal durations (e.g. 0.5s), and perform random angle selection in a way that minimizes target deviation from center, even after many dither operations.

### Plate Solving
- Added interface for ASTAP, the Astrometric STAcking Program, astrometric solver and FITS viewer as a plate solver
- Mid-sequence plate solve operations (when slewing to target, or after Meridian Flip) have been enhanced to have the following behavior:
  - If plate solve fails, it automatically falls back to blind failover
  - If blind failover also fails, plate solve can be set to await a certain time period (by default 10 minutes) before trying again, up to a certain number of attempts (user-defined)
  - If all attempts fail, or Meridian is getting close, plate solve will be considered failed, but sequence will continue as usual
- Added options to adjust downsample factor and maximum number of considered stars for ASTAP and local astrometry.net solvers

### GPS Assisted Location
- Added a NMEA GPS interface to retrieve the current location

### Interfacing with planetarium programs
- NINA can interface with Cartes du Ciel, HNSKY, Stellarium, and TheSkyX through their repsective TCP services to import the selected object for use in the Sequence Editor and Framing Assistant, as well as setting the observing location to match that which is set in those programs

### Manual Camera
- Inside the camera selection there is a new entry for "Manual Camera".
- This simulated camera will enable the use of cameras which lack an SDK, while still using the whole N.I.N.A. workflow.
- It will watch a specified folder for newly created files. These files will be stored inside an internal queue.
- Each time the application wants to download an exposure from the camera, the first item of this file queue is resolved and loaded into N.I.N.A.
- Additionally a manual Bulb Mode trigger can be activated, so it will use the selected Bulb Mode in Settings-&gt;Equipment-&gt;Camera on Exposure Start.
- When this trigger is deactivated the application will just skip the Start Exposure and wait for another file to roll into the specified folder

### Weather data sources
- The existing OpenWeatherMap implementation has been replaced with a full weather data interface.
- The new interface allows devices with ASCOM ObservingConditions class drivers to supply N.I.N.A. with weather data and other conditions.
- Native OpenWeatherMap functionality is maintained, and any configured OWM API key is retained and utilized by the new native OWM client.
- Weather data sources are now configured under the Equipment section.
- Any available weather data types (air temperature, pressure, wind speed, etc.) are inserted into images as FITS keywords and/or XISF image properties.

### Focusing
- Quick focuser movement buttons have been added (fine/coarse move IN/OUT) to the focuser views
- A new focuser settle time parameter has been added, in case the focuser shifts the image when moving (SCT, lens belt focusing, etc.). This should help with auto-focus in particular.
- Focuser backlash (in and out) can now be specified. The backlash will be applied to focuser movements whenever the focuser reverses directions.
- A new Measure Backlash tool has been added in the Auto-Focus view in the imaging tab. When launched, NINA will automatically measure focuser backlash IN and OUT.
- Sequence Auto-Focus can now be triggered if measured HFR of any frame is X% worse than the first frame taken after the previous auto-focus routine.
- More resilient autofocus:
  - Ability to automatically reattempt autofocus from scratch several times in case it failed
  - Automatically go back to original focus position if obtained HFR is significantly worse than original
  - Ability to take multiple autofocus exposures per focus point and average their HFR. This leads to smoother autofocus curves.
  - A crop ratio has been added to the autofocus, letting users autofocus only on the center of the frame
  - Autofocus can now be set to use only the top brightest stars, so the same stars are used throughout the autofocus routine. This will work best on sparse star fields, and for stable, well-aligned equipment (e.g. so stars don't move between auto-focus exposures)
  - Ability to use binning for autofocus

### Sequencing
- End of Sequence Options are now available, which include:
  - Parking the telescope - This will stop the guiding, and invoke the mount Park method if available, otherwise the mount will slew to near the celestial pole (on the same side of Meridian it last was) and stop tracking. **Before using this in production, test out the feature at the telescope, with your finger on the power switch. This is to avoid any crash into the pier for mounts that do not have limits.**
  - Warming the camera - the camera will be slowly cooled, with the cooler eventually turned off
- A pre-sequence check is now triggered, and will notify end users of a variety of potential issues (camera not cooled yet, guider not connected, telescope not connected but slew enabled, etc.) at sequence start
- It is now possible to reset the progress of a sequence item, or of a whole sequence target. If an item that occurred prior to the active sequence item is reset, stopping and starting the sequence will get back to it, but pausing/playing will keep going from the current item.
- The sequence start button is unavailable if an imaging loop is in progress in the imaging tab
- If telescope is capable of reporting SideOfPier there will now be a new option to consider this for calculating the need for meridian flips
- It is now possible to set the Offset in addition to the Gain within each sequence item
- Added buttons to move sequence row up and down the list 
- File handling now changed so that:
  - the default folder for sequences is set under Options -> Imaging
  - a 'modified' status is maintained for each target
  - targets can be loaded from any xml file
  - targets can be saved back to the file it was loaded from
  - a 'Save as' option is added to save to a new file
  - a warning is issued if a target is closed without saving when it has been modified.  This also applies when the application is closed.
- Controls to change order of targets in a multi-target sequence
- Ability to save and load 'target sets' (a set of targets in a certain sequence)

### Flat Wizard
- Progress bars have been added for remaining filters and exposures
- A new Slew to Zenith button has been added for easier flats. This includes an option for east or west pier side, depending on which side of pier the mount should approach zenith from.
- A new option to pause between filters has been added. This is to allow the user the chance to set lightbox settings 

### Interface
- Imaging tab - Equipment specific views will only show the "Connected" flag when the device is not connected to save space
- Added a layout reset button to the imaging tab to restore the default dock layout.
- Equipment chooser dropdowns are now grouped by driver categories to easily distinguish between for example ASCOM drivers and other vendor drivers

### File Handling
- All FITS keywords now have descriptive comments with units of measurement noted if applicable
- FITS keyword `DATE-OBS` now has millisecond resolution, eg: `2019-03-24T04:04:55.045`
- Additional FITS keywords are now added to images if their associated data is available:
	- `DATE-LOC`: Date and time of exposure adjusted for local time
	- `FOCRATIO`: Focal ratio of the telescope, user-configurable under Options->Equipment->Telescope
	- `SET-TEMP`: The configured CCD cooling setpoint
	- `SITEELEV`: Elevation of the observing site, in meters
	- `SWCREATE`: Contains `N.I.N.A. <version> <architecture>`
	- `TELESCOP`: Telescope name if provided under Options->Equipment->Telescope. Falls back to ASCOM mount driver name

### OSC Camera Handling
- Debayering is now applied prior to plate-solving or auto-focus star detection
- An Unlinked Stretch option has been added. When enabled, color channels will be stretched separately, helping hide the sky background. This results in more visible celestial objects, and helps enhance both autofocus and platesolving reliablity, especially in light polluted areas. Processing time is however increased.
- A Debayered HFR option has been added. When enabled, the HFR computation will be made on the Debayered image rather than the Bayered array, providing better Auto-focus results

### Star Detection Sensitivity
- Star Detection has been enhanced to detect more stars more accurately, while avoiding picking up noise by checking that the star is a local maximum and has sufficient bright pixels
- In addition, a new Star Sensitivity Parameter is available in the Imaging options. It has three settings:
	- Normal: use standard NINA star detection
	- High: More sensitive method, with little to no performance impact. Typically picks up 1.5x - 2.5x more stars than Normal
	- Highest: Most sensitive method, with some performance impact. Typically picks up 1.5x - 2.5x more stars than High
- The higher the detection level, the more likely lumps of noise are liable to be picked up (despite rejection parameters to avoid that)
- A noise reduction parameter has been added for better star detection in fairly noisy images, which can be important if using High or Highest star sensitivity levels, although the Normal sensitivity level will also benefit from it. Several settings are available:
	- None: no noise reduction on full size image done before star detection
	- Median: a Median filter is applied to the full size image before star detection. This is good if the sensor has many hot pixels, but time consuming
	- Normal: a standard fast Gaussian filter is applied to the full size image before star detection. This is good for smoothing out the thermal and bias noise of the sensor
	- High: a strong fast Gaussian filter is applied to the full size image before star detection. This is for fairly noisy images, but can make star detection more difficult
	- Highest: a very strong fast Gaussian filter is applied to the full size image before star detection. This is for noisy images, although star detection may suffer


## Bugfixes
- Fixed when FramingAssistant was not opened before and a DSO was selected from the SkyAtlas as Framing Source an error could occur
- Fixed scrolling through Framing Assistant Offline Sky Map while cursor was inside Rectangle ignored zooming
- Fixed Alitude charts displaying wrong Twilight/Night predictions for some scenarios
- Manual focus target list was not updating in some scenarios. Now it will always update. The interval for updates is one minute.
- Fixed an issue in FramingAssistant when reloading the image and having multiple panels selected, that the orientation was not considered properly resulting in wrong coordinates
- Fixed an issue in the Telescope Equipment tab that could potentially slew to the wrong Declination if the declination angle was negative
- Fixed issues in the subsampling logic for ASCOM, ZWO, and QHY cameras - the origin coordinates are now properly set, and take binning into account
- Fixed a race condition that caused HFR to not be computed for frames right before autofocus in some instances
- Fixed an issue for Nikon SDK that looked into the wrong folder for the external md3 files.
- Fixed a bug where Platesolve Orientation was displayed as negative and also throwed of the rotation centering when using rotators.
- Fixed a bug in Autofocus routine that could wrongly declare an autofocus run a failure if the starting point couldn't detect any stars
- Fixed custom color schema not saving properly and resetting to default when reloading the application
- The backlash measurement routine has been fixed so that the focuser is properly recentered before the backlashOUT measurement procedure
- Some Sky Surveys did not work in some locales due to decimal pointer settings 
- Fixed race condition when using DCRaw when the previous temp image was not finished processed and the new image tried to replace the previous temp image

## Improvements
- When EOS Utility is running in the background, the x64 N.I.N.A. client will scan for this app and prevent a crash due to the EOS utility being open. Instead a notification will show up to close the EOS Utility.
- N.I.N.A. SQLite Database will be created on demand and migrated to new versions on application startup instead of just being overwritten by the installer.
- Setup Installer can be run in fewer clicks and is also capable of launching the application after successful installation.
- Image History Graph will only contain statistics from sequence items
- Further increased parallelism during sequencing. 
	- After capture during image download: Parallel dither and change filter (if required)
	- After download: Parallel image saving and processing to display the image
- Image HFR is now available as an image file name token (`$$HFR$$`)
- Focuser Temperature is now available as an image file name token (`$$FOCUSERTEMP$$`)
- Added a clear button on HFR History graph. The button will be displayed when hovering the control.
- A new button is added in the options to directly open the log destination folder
- Added option to adjust USBLimit for Altair and Touptek cameras. This can prevent potential black preview screen issues. More details on this topic described at altair: https://altaircameras.com/black_preview_screen/

## Special Thanks
The N.I.N.A. team would like to sincerely thank:
- The staff at [Cloud Break Optics](https://cloudbreakoptics.com/) and [Finger Lakes Instrumentation](http://flicamera.com/) for arranging a ProLine PL09000 and CFW1-5 to assist in integrating native FLI camera and filter wheel support.
- Filippo Bradaschia from [PrimaLuceLab](https://www.primalucelab.com/) for providing an EAGLE unit to implement direct interfacing with the EAGLE Manager

## Included Camera SDK Versions:
- Altair: 39.15529.2019.906
- Atik: 8.7.3.5
- Canon: 3.8.20.0
- FLI: 1.104.0.0
- Nikon: 1.3.0.3001
- QHY: 0.5.1.0
- ToupTek: 30.13342.2018.1121
- ZWO: 1.14.7.15

# Version 1.8 Hotfix 1

- Prevent an unnecessary profile saving on application start
- Profiles where always saved after a sequence exposure, which slows down the image saving process. During sequence capture the profile saving is disabled now.

# Version 1.8

## Features

### Sequence
- Enable/Disable sequence entries

### Plate Solving
- Added interface for All Sky Platesolver

### Camera Control
- Altair native driver support
- ToupTek native driver support
- QHYCCD native driver support
- Added support for anti-dew heaters in ZWO cameras
- On ASCOM drivers support for setting readout modes

### Framing Assistant
- Add SkyAtlas image source which allows for framing based on offline SkyAtlas data

### Flat Wizard
- Supports you taking flats

### Imaging
- Added a list of manual focus targets (bright stars) that are currently visible in the sky

### Framing Assistant Offline Sky Chart
- Based on Sky Atlas data a basic sky chart showing objects can be displayed
- Instead of dragging the rectangular through the initial image like in the other framing sources
  the background itself will be moved like in an orrery  

### Guiding
 - Added Synchronized PHD2 Guider (experimental)
 - Synchronized PHD2 Guider will synchronize your Dither requests between multiple instances of N.I.N.A.
	- Known limitations: Dithering will happen every possible synchronized frame and is not changeable

### Auto-Update Channels
 - Previously the auto update was always just looking for released versions. Now multiple sources (Release, Beta and Nightly) can be selected and the auto updater updates to the respective version accordingls.
 - Additionally the changelog for the new version will now be displayed prior to updating, too. 


## Bugfixes

- Corrected Max Binning level for ASI Cameras
- Focuser move command fixed where lots of move commands where sent by accident
- Rotation value now considered for sequence import|export
- Canon: Fixed bulb mode for exposure times <30s
- Canon: All shutter speeds now correctly added when step set custom function is set to 1/3
- Meridian Flip window does not get stuck anymore when clicking on cancel
- Log Level will now be set on application start based on profile settings.
- ASI Cameras will not shut down their cooling and progress on opening multiple instances of NINA
- In some cases the application stayed open in the background after closing. This should not happen anymore
- `$$DATETime$$` and `$$TIME$$` will now use timestamp on exposure start, not on exposure end
- Fixed Meridian Offset default values for plate solved polar alignment 
- On sequence target guiding will be paused, prior to slewing to a new target
- Autofocus will not trigger a pause/resume command anymore, as this was not reliable in some cases. Instead PHD2 is stopped and started, similar to what is done during meridian flip.
- Fixed Platesolve result reporting wrong Dec Error.
- Platesolve recenter now considers proper angular distance calculation instead of relying just on Right Ascension.

## Improvements

- Clear button for PHD2 Graph
- Hide camera cooler controls when not available for current camera model
- Zero Floating point numbers now displayed as "0.00" instead of ".00"
- Show better exception message when an ASCOM Interop Exception occurs
- Canon: Errors are now shown to users in a readable format if any occur
- Removed hard requirement of ASCOM platform. Application can now function without it
- Improved UI Style. 
    - New logo
    - Better version display
    - Tweaked some color themes for more consistent colors
    - Better spacing between elements to reduce wasted space
    - Two new background colors to better pronounce some ui elements
    - Reworked Imaging tab to have a common style.
    - Imaging tab tools pane (to hide/show panels) moved to the top and split into two separate categories
- Profiles don't get overriden when using multiple instances of N.I.N.A. with each one having a separate profile active
- Autostretch replaced by a better midpoint transformation function
- Autostretch now has black point clipping options
- Vastly improved image statistics calculation time.
- Estimated Finish Time will automatically update in the sequencing view
- Added copy button for existing color schemas to copy over to custom and alternative custom schemas    
- Framing tab: 
	- Moved coordinates out of framing boxes to not obscure target
	- Added a new button to be able to add the framing target to a sequence instead of replacing
	- Control to adjust opacity of framing box
- Improved Framing Assistant and Sequence Target Textboxes by giving up to 50 target hints based on input to select from
- Framing Assistant now can annotate DSO
- Sensor offset is now available as an image file name token (`$$OFFSET$$`)
- Attempt to start PHD2 and connect all equipment when connecting to guider and PHD2 is not running
- Adaptive Cooling: Duration for cool/warm camera is now a minimum duration. In case the cooler cannot keep up with the set duration, the application will wait for the camera to reach the checkpoints instead of just continuing setting new targets without the camera having any chance to reach those in the timeframe.
- Automatically import filter wheel filters to the profile on connection when profile filter list is still empty
- Load a default imaging tab layout in case the layout file is corrupted or not compatible anymore
- Removed Altitude Side combobox from plate solved polar alignment. It will be automatically determined based on alt/az coordinates.
- Changed log file format. Each application start will write to a separate log file for better distinction
- Improved XISF save speed and resulting file size by not embedding the image as base64 string, but instead as attached raw byte data
- Additional FITS keywords are now added to images if their associated data is available:
	- `OBJECT`: The name of the target, as supplied in the active Sequence
	- `DEC` and `RA`: The DEC and RA of the telescope at the time of the exposure
	- `INSTRUME`: The name of the connected camera
	- `OFFSET`: The sensor offset, if applicable
	- `FWHEEL`: Name of the connected filter wheel
	- `FOCNAME`: Name of the connected focuser
	- `FOCPOS` and `FOCUSPOS`: Position of the focuser, in steps
	- `FOCUSSZ`: Size of a focuser step, in microns
	- `FOCTEMP` and `FOCUSTEM`: Temperature reported by the focuser
	- `ROTNAME` Name of the connected rotator
	- `ROTATOR` and `ROTATANG`: Angle of the rotator
	- `ROTSTPSZ` Minimum rotator step size, in degrees
	- Applicable XISF Image Property analogs of the above, as defined by XISF 1.0 Section 11.5.3

## Special Thanks
The N.I.N.A. team would like to thank 

- Nick Smith from [Altair Astro](https://www.altairastro.com/) for providing a GPCAM2 290C to integrate Altair SDK
- Elias Erdnuess from [Astroshop.eu](https://www.astroshop.eu) for providing multiple Toupcam Cameras to integrate ToupTek SDK
- The staff at QHYCCD dealer [Cloud Break Optics](https://cloudbreakoptics.com/) for lending a QHY183C to integrate QHYCCD SDK

These items helped a lot during development and testing.  
Thank you for your support!

___

# Version 1.7

## Features

### Framing Assistant
- Mosaic planning for framing assistant
- Added multiple new SkySurveys to choose from for framing
- DSLR RAW files can now be loaded into framing assistant

### Sequences
- Sequence multiple target planning. Import/Export also available 
- Consider Rotation for framing assistant when set for a sequence during platesolves

### Filter Wheel
- Added a manual FilterWheel for users without a motorized wheel or with a filter drawer. There will pop up a window and prompt the user when a filter change is requested

### Rotators
- Support for ASCOM Rotators added
- A manual rotator option is added, for users without an automatic rotator. A pop up will show with the current angle and target angle for a user to manually rotate to

### Imaging
- DSLR Users: Images will now always be saved as RAW to prevent any loss of data due to conversions
- Aberration inspector added to imaging tab. It will show a 3x3 Panel containing the current image
- Battery display for DSLRs

### Settings
- Added ImageParameter `$$RMSARCSEC$$` and `$$FOCUSPOSITION$$`
- Latitude and Longitude can now be synced from application to telescope and vica versa (when supported).
- Serial Relay (via USB) interaction for Nikon Bulb

## Bugfixes

- Fixed Canon issue that a second exposure is accidentally triggered, causing the camera to be stuck
- Fixed memory leak when using Free Image RAW Converter

## Improvements

- Guider is also connected when pushing "Connect All"
- Changed FilterWheel UI for switching filters
- Some improvements to memory consumption
- Automatically triggered Autofocus and Platesolving now spawns inside a separate window
- Guiding Dither thresholds now configurable
- Flipped + and - for Stepper Controls
- Minor UI Layout improvements
- Made SkySurvey Cache directory configurable
- Local astrometry.net client will now downscale image for faster image solving
- ImagePatterns inside options can now be dragged from the list to the textbox
- Major code refactorings for better maintainability
- Lots and lots of minor bugfixes and improvements
