using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoGameStore.Data;
using VideoGameStore.Models;

namespace VideoGameStore.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var cartItems = await _context.CartItems
                .Include(c => c.Game)
                .Where(c => c.UserId == user!.Id)
                .ToListAsync();

            return View(cartItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int gameId)
        {
            var user = await _userManager.GetUserAsync(User);

            var alreadyOwned = await _context.Purchases
                .AnyAsync(p =>
                    p.UserId == user!.Id &&
                    p.GameId == gameId);

            if (alreadyOwned)
            {
                return RedirectToAction("Details", "Games", new { id = gameId });
            }

            var alreadyInCart = await _context.CartItems
                .AnyAsync(c =>
                    c.UserId == user!.Id &&
                    c.GameId == gameId);

            if (!alreadyInCart)
            {
                var cartItem = new CartItem
                {
                    UserId = user.Id,
                    GameId = gameId
                };

                _context.CartItems.Add(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}