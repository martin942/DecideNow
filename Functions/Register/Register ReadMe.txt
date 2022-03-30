Register
----------

Urls:
  POST:
    https://europe-west1-decidenow-344308.cloudfunctions.net/register
    BODY:
      {
          "userName":"<username>",
          "email":"<email>",
          "password":"<password-hash>"
      }
      
      on Error:
      { 
          "message":<message>,
          "statusCode":<statusCode>
      }
      
      minimale benutzername länge 8 zeichen
