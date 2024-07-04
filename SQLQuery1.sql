use TourDb
SELECT
    sum(t.RentPerPerson + f.Price + g.GuideFee + h.PricePerNight) AS totalprice
FROM TourPackages p
JOIN TourTransports t ON p.TourTransportId = t.TourTransportId
JOIN TourFoods f ON p.TourFoodId = f.TourFoodId
JOIN TourGuides g ON p.TourGuideId = g.TourGuideId
JOIN TourHotels h ON p.TourHotelId = h.TourHotelId
Where p.TourPackageId = 1

select t.TourTransportId,RentPerPerson 
from TourPackages p join TourTransports t
on p.TourTransportId=t.TourTransportId
where p.TourPackageId=2
go

select p.TotalAmount 
from Tours t join TourPackages p
on t.TourPackageId=p.TourPackageId
where t.TourId=2
go
select * from Tours

select * from TourPackages

select p.Discount 
from TourBookings t join Promotions p
on t.PromotionId=p.PromotionId
where t.TourBookingId=1

select Discount from Promotions
where PromotionId=2

select * from Promotions
select * from TourBookings
select * from TourAvailabilities

select a.AvailableSlots 
from TourBookings b join TourAvailabilities a
on b.TourId=a.TourId
where b.TourId=2
