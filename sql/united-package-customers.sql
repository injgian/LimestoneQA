-- Task: all customer names and countries that used "United Package" as their shipper.
-- Run in the W3Schools Try-It editor (Northwind sample database).
--
-- Notes on the shape of the answer:
--   * Customers -> Orders is one-to-many, so a customer with several United Package
--     orders would appear several times. DISTINCT collapses that to one row per customer.
--   * The shipper is matched by name rather than by the hard-coded id 2, so the query
--     still reads correctly if the reference data is reordered.

SELECT DISTINCT
    c.CustomerName,
    c.Country
FROM Customers AS c
INNER JOIN Orders AS o
    ON o.CustomerID = c.CustomerID
INNER JOIN Shippers AS s
    ON s.ShipperID = o.ShipperID
WHERE s.ShipperName = 'United Package'
ORDER BY c.CustomerName;
