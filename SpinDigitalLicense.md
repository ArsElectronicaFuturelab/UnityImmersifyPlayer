# License for Spin Digital HEVC Decoder
This Unity Plugin employs a HEVC decoder that has been developed by Spin Digital Video Technologies GmbH. The Spin Digital HEVC decoder library (__spinlib_rms.dll__) is provided as free-of-charge evaluation license and is deployed using a cloud-based lease license. The actual license is deployed on the cloud license server, and the local target system leases a license from the license server by license synchronization. The synchronized local license can be used offline for a maximumm of 30 days without an internet connection.

For the first use, a license initialization is required. Afterward, the license will be automatically synchronized every time the _spinlib_rms.dll_ library is loaded and every hour during the runtime.

The license initialization and synchronization requires an internet connection.


## __Demo License__
For testing purposes, an evaluation (demo) license

  > * __Entitilement ID:__ ea08c92c-cb40-4482-b188-c5bf02c2018f
  > * __Customer ID:__     4605c9f6-0fa8-40be-b9f3-016048c70f2b

is provided free-of-charge but with some restrictions on the supported features:
  - Max. decoding resolution: 8192×8192 pixels
  - Chroma format and bit-depth: 4:2:0, 10-bit
  - Watermark (Spin Digital logo) on the decoded pictures
  - Valid until 2020-12-31

This evaluation license is valid until the end of 2020. For the further releases of the Unity plugin with an updated _spinlib_rms.dll_, a new evaluation license will be provided.

### __License initialization__
To initialize the license, you will need:
  - The entitlement information of the demo license.
  - A base license file _lservrc_ provided in the repository. It will be later updated in order to store the actual license information used by the Spin Digital HEVC decoder library.
  - The Spin Digital license tool _spinlicensetool_rms.exe_

The instructions for initializing the license are provided below:
1. Make sure that the _lservrc_ file and the _spinlicensetool_rms.exe_ are placed in the same folder where _spinlib_rms.dll_ is located.
2. Open the _Windows PowerShell_ or  _Command Prompt_ with the __Administrator Privileges__
3. Go the directory of _lservrc_ and the _spinlicensetool_rms.exe_ and execute the following command:

   `.\spinlicensetool_rms.exe --init --lease sync --eid ea08c92c-cb40-4482-b188-c5bf02c2018f --customer 4605c9f6-0fa8-40be-b9f3-016048c70f2b`

   This command will create a configuration file  _sntlcloudp_configuration_spindigitalvideotechnologiesgmbh.xml_ and update the _lservrc_ file with the actual license information.
4. Now you would be able to use the HEVC decoder within the Unity plugin.

> Note:
> * Please keep the configuration file _sntlcloudp_configuration_spindigitalvideotechnologiesgmbh.xml_ and _lservrc_ in the same folder as the _spinlib_rms.dll_, so the license synchronization can be performed automatically for future use.


### __License synchronization__
Usually, manual license synchronization is not needed as the license synchronization is performed automatically after license initialization.  However, a force license synchronization can be performed manually if any license issues occur:
1. Make sure the _sntlcloudp_configuration_spindigitalvideotechnologiesgmbh.xml_, _lservrc_ and _spinlicensetool_rms.exe_ are in the same folder.
2. Open the _Windows PowerShell_ or _Command Prompt_
3. Go to the directory where the _spinlicensetool_rms.exe_ is located, and run following command:

   `.\spinlicensetool_rms.exe --lease sync`

   This command will synchronize the license and update license in the _lservrc_ file.

> Note:
> * Administrator Privileges are not required for license synchronization.


## __Subscription license__

To upgrade the evaluation license to a commercial and end-user license you will need to acquire a license from Spin Digital. Please contact the Spin Digital team (<unity3d@spin-digital.com>) for more information.