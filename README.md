# Open PGP Explorer (Opex)

View file structure of any Open PGP file as per [RFC 4880](https://tools.ietf.org/html/rfc4880)

### This tool lets you

* View any Open PGP file structure
* View Secret Key material
* Extract Open PGP Messages
* Verify Signatures

In cases where the public key is not present in the file, the signature packet will be shown in red, adding the public key file later will re-validate the signature.

*Caution*
Please Note this tool does not make any attempt to protect decrypted keys and decrypted data so please use with caution.


### Acknowledgement

This tool uses the BZip2 extraction library from
[SharpZipLib](https://github.com/icsharpcode/SharpZipLib)

### Contact Us
It is a work in progress, please email me with bugs or suggestions at [support@titasraha.com](mailto:support@titasraha.com)

More to come...