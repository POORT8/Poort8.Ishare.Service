Poort8.Ishare.Service


# Testing

The Poort8.Ishare.Service container can be tested using a the Postman test collection `Poort8.Ishare.Service.postman_collection.json`. After obtaining an iSHARE test certificate, one can directly try the Poort8 implementation of Poort8.Ishare.Common and Poort8.Ishare.Service.

Then, by changing the collection variables, one can use this Postman collection to test your own implementation.


### How does one use it?

1. [Get Postman](https://www.getpostman.com/apps)
2. Run it. Don't bother signing in if you don't want to, there's a small link on the bottom to skip. This project does not use any of Postman's cloud features.
3. Click `Import` button in top left and drag `Poort8.Ishare.Service.postman_collection.json there.
4. Open the collection `Sample Service Provider Calls` and go to the tab `Variables`. Replace serviceConsumer.EORI with the EORI number from the iSHARE test certificate in the format `EU.EORI.NL_________`.
5. Also in the tab `Variables`, add your iSHARE public and private key in the designated variables. This is used in the test collection for obtaining the iSHARE required client assertion. NB. this is not good practice for one's operational implementation. Therefore ONLY do this with test certificates, do not add the private key from any operational certificate. 
6. Click `Run`
7. After implementing `Poort8.Ishare.Common` and `Poort8.Ishare.Service`, one can edit the serviceProvider variables to match the details of one's own implementation.

### How does it work?

Postman automatically runs a series of scripts to handle the iSHARE-defined Identification and Authorization procedures:
- as step 0., a sample Delegation Evidence is obtained from the Poort8 authorization registry. This sample allows the serviceConsumer from step 4. to obtain the data on behalf of dummy organisation EU.EORI.NL888888882, who is allowed to `read` the attribute `test` of item `1` in `poort8.iSHARE.service` for `IntegrationTesting`.
- then in step 1. an access_token is obtained from the serviceProvider
- in step 2. - using both results from 0. and 1. - the service from the serviceProvider is called.
Automated javastript tests check if the calls are successful
