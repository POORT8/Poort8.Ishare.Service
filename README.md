[![Actions Status](https://github.com/POORT8/Poort8.Ishare.Common/workflows/Build%20and%20test/badge.svg)](https://github.com/POORT8/Poort8.Ishare.Common/actions)

# Poort8.Ishare.Service
The project contains the *Service Provider endpoints* of the [iSHARE scheme](https://dev.ishareworks.org/):

 - [Service](https://dev.ishareworks.org/service-provider/service.html)

 This is an abstract endpoint, that can be implemented multiple times, for each business specific service. 
 
 ## Requirements 
 - System requirements
   - Docker
 - Each `Service` endpoint must be called using an iSHARE `access_token` obtained from the service provider as an authentication method. The token endpoint can be implemented using the repo `https://github.com/POORT8/Poort8.Ishare.Common`.
 - Both `https://github.com/POORT8/Poort8.Ishare.Common` and `https://github.com/POORT8/Poort8.Ishare.Service` have a dependency on the nuget package `Poort8.Ishare.Core`, but require different versions. Implement both containers using Docker Compose to keep these dependecies for each container in tact.
 - Optionally, a call to the `Service` can be set to require `delegation_evidence` from an iSHARE authorization registry as an authorization method.

## Getting Started

TBD

## Demo and testing
In the context of git repos testing is usually referred to as unit/integration testing. Here it means (for the lack of a better term) playing with the endpoint

The Poort8.Ishare.Service container can be tested using the Postman test collection `Poort8.Ishare.Service.postman_collection.json`. After obtaining an iSHARE test certificate, one can directly try the Poort8 implementation of Poort8.Ishare.Common and Poort8.Ishare.Service.

Then, by changing the collection variables, one can use this Postman collection to test your own implementation.

### How does one use it?

1. [Get Postman](https://www.getpostman.com/apps)
2. Run it. Don't bother signing in if you don't want to, there's a small link on the bottom to skip. This project does not use any of Postman's cloud features.
3. Click `Import` button in top left and drag `Poort8.Ishare.Service.postman_collection.json` there.
4. Open the collection `Sample Service Provider Calls` and go to the tab `Variables`. Replace serviceConsumer.EORI with the EORI number from the iSHARE test certificate in the format `EU.EORI.NL_________`.
5. Also in the tab `Variables`, add your iSHARE public and private key in the designated variables. 
  - NB. In the test collection this is sent to an endpoint from the iSHARE scheme owner to obtain the iSHARE required client assertion. *This means the submitted private key is sent over the internet*. This is not good practice for one's operational implementation. Therefore ONLY do this with test certificates, do not add the private key from any operational certificate. 
  - NB2. Retrieving public and private keys from the test certificate can be cumbersome. The iSHARE foundation provides a code snippet to support this process here: https://github.com/iSHAREScheme/code-snippets/tree/master/Cert_Key_Extractor.
  - Use the public key _without_ linebreaks and _without_ begin and end:
```
MIID****
```
  - Use the private key _exactly_ in this format, including begin, line breaks and end:
```
-----BEGIN PRIVATE KEY----- 
MIIE***** 
-----END PRIVATE KEY-----
```

7. Click `Run`
8. After implementing `Poort8.Ishare.Common` and `Poort8.Ishare.Service`, one can edit the serviceProvider variables to match the details of one's own implementation.

### How does it work?

Postman automatically runs a series of scripts to handle the iSHARE-defined Identification and Authorization procedures:
- as step 0., a sample Delegation Evidence is obtained from the Poort8 authorization registry. This sample allows the serviceConsumer from step 4. to obtain the data on behalf of dummy organisation `EU.EORI.NL888888882`, who is allowed to `read` the attribute `test` of item `1` in `poort8.iSHARE.service` for `IntegrationTesting`.
- then in step 1. an access_token is obtained from the serviceProvider
- in step 2. - using both results from 0. and 1. - the service from the serviceProvider is called.
Automated javastript tests check if the calls are successful

## Acknowledgements

This package was developed with partial funding from the Dutch Topsector Logistics.
