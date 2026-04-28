using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoGameStore.Data;
using VideoGameStore.Models;
using VideoGameStore.Services.Payments;

namespace VideoGameStore.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentService _paymentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentsController(ApplicationDbContext context, IPaymentService paymentService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _paymentService = paymentService;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(int gameId)
        {
            var game = await _context.Games.FindAsync(gameId);

            if (game == null || game.Price == null)
                return BadRequest();

            var returnUrl = Url.Action(
                "CaptureOrder",
                "Payments",
                new { gameId = gameId },
                Request.Scheme);

            var cancelUrl = Url.Action(
                "Cancel",
                "Payments",
                null,
                Request.Scheme);

            var approvalUrl = await _paymentService.CreateOrderAsync(
                game.Price.Value,
                returnUrl,
                cancelUrl
            );

            return Redirect(approvalUrl);
        }

        public async Task<IActionResult> Success(int gameId)
        {
            var game = await _context.Games.FindAsync(gameId);

            return View(game);
        }

        public async Task<IActionResult> CaptureOrder(string token, int gameId)
        {
            var success = await _paymentService.CaptureOrderAsync(token);

            if (!success)
                return BadRequest("Payment failed");

            var userId = _userManager.GetUserId(User);

            var game = await _context.Games.FindAsync(gameId);

            var purchase = new Purchase
            {
                GameId = gameId,
                UserId = userId,
                Price = game.Price.Value,
                PayPalOrderId = token
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            return RedirectToAction("Success", new { gameId = gameId });
        }

        [Authorize]
        public async Task<IActionResult> MyPurchases()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
                return Unauthorized();

            var purchases = await _context.Purchases
                .Include(p => p.Game)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(purchases);
        }

        public IActionResult Cancel()
        {
            return View();
        }
    }
}
