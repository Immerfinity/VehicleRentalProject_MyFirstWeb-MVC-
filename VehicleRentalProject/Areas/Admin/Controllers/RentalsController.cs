using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleRentalProject.Repositories.Infrastructure;
using AutoMapper;
using VehicleRentalProject.Web.Models.ViewModels.Rental;
using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using VehicleRentalProject.Repositories.Implementation;
using iTextSharp.text.pdf;
using iTextSharp.text;



namespace VehicleRentalProject.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RentalsController : Controller
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public RentalsController(
            IRentalRepository rentalRepository,
            IMapper mapper
,
            IUserService userService)
        {
            _rentalRepository = rentalRepository;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var rentals = await _rentalRepository.GetAllRentals();
            var rentalViewModels = _mapper.Map<List<RentalHistoryViewModel>>(rentals);

            return View(rentalViewModels);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var rental = await _rentalRepository.GetRentalById(id);
            var applicationUser = _userService.GetApplicationUser(rental.ApplicationUserId);
            var updateStatusViewModel = _mapper.Map<UpdateRentalStatusViewModel>(rental);
            return View(updateStatusViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(UpdateRentalStatusViewModel vm)
        {

            var rental = await _rentalRepository.GetRentalById(vm.Id);
            rental.RentalStatus = vm.Status;

            if (vm.PenaltyAmount > 0)
            {
                rental.PenaltyAmount = vm.PenaltyAmount;
            }
            await _rentalRepository.UpdateRental(rental);

            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> FinedUsersReport()
        {
            var finedRentals = await _rentalRepository.GetFinedRentalsAsync();
            var vm = finedRentals.Select(r => new FinedUserViewModel
            {
                RentalId = r.Id,
                VehicleNumber = r.Vehicle.VehicleNumber,
                StartDate = r.StartDate,
                ReturnDate = r.ReturnDate,
                PenaltyAmount = r.PenaltyAmount,
                FullName = r.ApplicationUser.FullName,
                UserName = r.ApplicationUser.UserName
            }).ToList();

            return View(vm);
        }

        public async Task<IActionResult> ExportFinedUsersReportToPDF()
        {
            var finedRentals = await _rentalRepository.GetFinedRentalsAsync();
            var vm = finedRentals.Select(r => new FinedUserViewModel
            {
                RentalId = r.Id,
                VehicleNumber = r.Vehicle.VehicleNumber,
                StartDate = r.StartDate,
                ReturnDate = r.ReturnDate,
                PenaltyAmount = r.PenaltyAmount,
                FullName = r.ApplicationUser.FullName,
                UserName = r.ApplicationUser.UserName
            }).ToList();

            using (var stream = new MemoryStream())
            {
                Document document = new Document();
                PdfWriter.GetInstance(document, stream).CloseStream = false;

                document.Open();
                document.Add(new Paragraph("Отчет о всех оштрафованных пользователях", GetRussianFont()));

                document.Add(new Paragraph("\n", GetRussianFont()));
                document.Add(new Paragraph("\n", GetRussianFont()));

                PdfPTable table = new PdfPTable(6);
                table.AddCell(new PdfPCell(new Phrase("Номер аренды", GetRussianFont())));
                table.AddCell(new PdfPCell(new Phrase("Номер автомобиля", GetRussianFont())));
                table.AddCell(new PdfPCell(new Phrase("Дата начала", GetRussianFont())));
                table.AddCell(new PdfPCell(new Phrase("Дата возврата", GetRussianFont())));
                table.AddCell(new PdfPCell(new Phrase("Штрафная сумма", GetRussianFont())));
                table.AddCell(new PdfPCell(new Phrase("Пользователь", GetRussianFont())));

                foreach (var rental in vm)
                {
                    table.AddCell(new PdfPCell(new Phrase(rental.RentalId.ToString(), GetRussianFont())));
                    table.AddCell(new PdfPCell(new Phrase(rental.VehicleNumber, GetRussianFont())));
                    table.AddCell(new PdfPCell(new Phrase(rental.StartDate.ToString("dd.MM.yyyy"), GetRussianFont())));
                    table.AddCell(new PdfPCell(new Phrase(rental.ReturnDate?.ToString("dd.MM.yyyy"), GetRussianFont())));
                    table.AddCell(new PdfPCell(new Phrase(rental.PenaltyAmount.ToString("C"), GetRussianFont())));
                    table.AddCell(new PdfPCell(new Phrase($"{rental.FullName} ({rental.UserName})", GetRussianFont())));
                }

                document.Add(table);
                document.Close();

                byte[] bytes = stream.ToArray();
                stream.Close();

                return File(bytes, "application/pdf", "FinedUsersReport.pdf");
            }
        }

        public async Task<IActionResult> GenerateRentalAgreement(int id)
        {
            var rental = await _rentalRepository.GetRentalById(id);
            var applicationUser = _userService.GetApplicationUser(rental.ApplicationUserId);

            using (var stream = new MemoryStream())
            {
                Document document = new Document();
                PdfWriter.GetInstance(document, stream).CloseStream = false;

                document.Open();
                document.Add(new Paragraph("Договор аренды транспортного средства", GetRussianFont()));

                document.Add(new Paragraph("\n", GetRussianFont()));
                document.Add(new Paragraph("\n", GetRussianFont()));

                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 1, 2 });

                table.AddCell(new PdfPCell(new Phrase("Арендодатель:", GetRussianFont())));
                table.AddCell(new PdfPCell(new Phrase("             ", GetRussianFont())));

                document.Add(new Paragraph("\n\n", GetRussianFont()));

                table.AddCell(new PdfPCell(new Phrase("Арендатор:", GetRussianFont())));
                table.AddCell(new PdfPCell(new Phrase(applicationUser.FullName, GetRussianFont())));
                table.AddCell(new PdfPCell(new Phrase("Дата рождения:", GetRussianFont())));
                table.AddCell(new PdfPCell(new Phrase("              ", GetRussianFont())));
                table.AddCell(new PdfPCell(new Phrase("Адрес:", GetRussianFont())));
                table.AddCell(new PdfPCell(new Phrase(applicationUser.Address, GetRussianFont())));

                table.AddCell(new PdfPCell(new Phrase("Транспортное средство:", GetRussianFont())));
                table.AddCell(new PdfPCell(new Phrase(rental.Vehicle.VehicleNumber, GetRussianFont())));

                table.AddCell(new PdfPCell(new Phrase("Дата начала аренды:", GetRussianFont())));
                table.AddCell(new PdfPCell(new Phrase(rental.StartDate.ToString("dd.MM.yyyy"), GetRussianFont())));

                table.AddCell(new PdfPCell(new Phrase("Дата окончания аренды:", GetRussianFont())));
                table.AddCell(new PdfPCell(new Phrase(rental.ReturnDate?.ToString("dd.MM.yyyy"), GetRussianFont())));

                table.AddCell(new PdfPCell(new Phrase("Стоимость аренды:", GetRussianFont())));
                table.AddCell(new PdfPCell(new Phrase(rental.TotalPrice.ToString("C"), GetRussianFont())));

                document.Add(table);
                document.Add(new Paragraph("\n\n", GetRussianFont()));

                document.Add(new Paragraph("Договор аренды транспортного средства", GetRussianFont()));
                document.Add(new Paragraph("1.1. Предметом настоящего договора является транспортное средство, далее ТС:", GetRussianFont()));
                document.Add(new Paragraph($"Марка, модель {rental.Vehicle.VehicleName} {rental.Vehicle.VehicleModel}", GetRussianFont()));
                document.Add(new Paragraph($"Тип транспортного средства по ПТС {rental.Vehicle.VehicleType}", GetRussianFont()));
                document.Add(new Paragraph($"Цвет {rental.Vehicle.VehicleColor}", GetRussianFont()));
                document.Add(new Paragraph("1.2. Указанное ТС принадлежит Арендодателю на основании паспорта транспортного средства.", GetRussianFont()));
                document.Add(new Paragraph("2.1. Арендодатель обязуется:", GetRussianFont()));
                document.Add(new Paragraph("2.1.1. Передать ТС Арендатору не позднее дня, следующего за днем подписания настоящего договора;", GetRussianFont()));
                document.Add(new Paragraph("2.1.2. Представить Арендатору транспортное средство в исправном техническом состоянии, без дефектов;", GetRussianFont()));
                document.Add(new Paragraph("2.1.3. Передать Арендатору технический паспорт транспортного средства и ключи от данного транспортного средства;", GetRussianFont()));
                document.Add(new Paragraph("2.1.4. Производить страхование транспортного средства за свой счет.", GetRussianFont()));
                document.Add(new Paragraph("2.2. Арендодатель вправе:", GetRussianFont()));
                document.Add(new Paragraph("2.2.1. Требовать от Арендатора своевременного внесения арендной платы;", GetRussianFont()));
                document.Add(new Paragraph("2.2.2. Требовать возврата арендованного транспортного средства в срок, установленный настоящим договором.", GetRussianFont()));
                document.Add(new Paragraph("3.1. Арендатор обязуется:", GetRussianFont()));
                document.Add(new Paragraph("3.1.1. Своевременно вносить арендную плату и использовать транспортное средство по назначению;", GetRussianFont()));
                document.Add(new Paragraph("3.1.2. Поддерживать транспортное средство в надлежащем состоянии и нести расходы на его содержание, а также расходы, связанные с его эксплуатацией в течение всего срока аренды;", GetRussianFont()));
                document.Add(new Paragraph("3.1.3. Осуществлять за свой счет капитальный и текущий ремонт переданного в аренду транспортного средства;", GetRussianFont()));
                document.Add(new Paragraph("3.1.4. Осуществлять управление транспортным средством, его техническую и коммерческую эксплуатацию своими силами;", GetRussianFont()));
                document.Add(new Paragraph("3.1.5. Производить за свой счет страхование ответственности за вред, причиненный третьим лицам в связи с использованием транспортного средства;", GetRussianFont()));
                document.Add(new Paragraph($"3.1.6. В течение {(TimeSpan)(rental.ReturnDate - rental.StartDate)} дней по истечении срока договора возвратить транспортное средство Арендодателю в исправном техническом состоянии с учетом нормального износа и без косметических дефектов.", GetRussianFont()));
                document.Add(new Paragraph("3.2. Арендатор вправе:", GetRussianFont()));
                document.Add(new Paragraph("3.2.1. Сдавать транспортное средство в субаренду с письменного согласия Арендодателя;", GetRussianFont()));
                document.Add(new Paragraph("3.2.2. Заключать от своего имени с третьими лицами договоры перевозки и иные договоры, не противоречащие назначению транспортного средства.", GetRussianFont()));
                document.Add(new Paragraph("4.2. Арендатор имеет преимущественное право заключения договора аренды на новый срок.", GetRussianFont()));
                document.Add(new Paragraph($"4.3. Арендная плата составляет {rental.TotalPrice} рублей в день.", GetRussianFont()));
                document.Add(new Paragraph("4.4. Сроки уплаты арендной платы:", GetRussianFont()));
                document.Add(new Paragraph("4.5. Арендатор производит платеж, не позднее последнего числа месяца, в котором осуществляется пользование транспортным средством.", GetRussianFont()));
                document.Add(new Paragraph($"4.6. В случае просрочки возврата транспортного средства Арендатор выплачивает Арендодателю неустойку в размере {rental.Vehicle.DailyRate} за каждый день просрочки.", GetRussianFont()));
                document.Add(new Paragraph("\n\n", GetRussianFont()));
                document.Add(new Paragraph("Арендодатель", GetRussianFont()));
                document.Add(new Paragraph("____________________________________________", GetRussianFont()));
                document.Add(new Paragraph("(подпись и ФИО)", GetRussianFont()));
                document.Add(new Paragraph("Тел.__________________________________________", GetRussianFont()));
                document.Add(new Paragraph("\n\n", GetRussianFont()));
                document.Add(new Paragraph("Арендатор", GetRussianFont()));
                document.Add(new Paragraph("____________________________________________", GetRussianFont()));
                document.Add(new Paragraph("(подпись и ФИО)", GetRussianFont()));
                document.Add(new Paragraph("Тел.__________________________________________", GetRussianFont()));

                document.Close();

                byte[] bytes = stream.ToArray();
                stream.Close();

                return File(bytes, "application/pdf", "RentalAgreement.pdf");
            }
        }


        private static Font GetRussianFont()
        {
            string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "Arial.ttf");
            BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            return new Font(baseFont, 12);
        }

    }
}
