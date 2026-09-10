# 1 Path pour JS

Dans "customers.js", le "const INDEX_URL = "/Customers";" est dangereux en production car la base url peut varier. Il est préférable de passer par l'helper url razor pour stocker la base url de Index.cshtml dans un data attribute par exemple et de le récupérer dans "customers.js".
