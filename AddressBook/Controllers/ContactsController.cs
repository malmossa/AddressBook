﻿using AddressBook.Data;
using AddressBook.Enums;
using AddressBook.Models;
using AddressBook.Models.ViewModels;
using AddressBook.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AddressBook.Controllers
{
    [Authorize]
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IAddressBookService _addressBookService;
        private readonly IEmailSender _emailService;

        public ContactsController(ApplicationDbContext context,
                                  UserManager<AppUser> userManager,
                                  IImageService imageService,
                                  IAddressBookService addressBookService,
                                  IEmailSender emailService)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _addressBookService = addressBookService;
            _emailService = emailService;
        }

        // GET: Contacts
        public IActionResult Index(int categoryId, string swalMessage = null)
        {
            ViewData["SwalMessage"] = swalMessage;

            List<Contact> contacts = new List<Contact>();
            string appUserId = _userManager.GetUserId(User);

            // return the user id and their associated contacts and categories
            AppUser appUser = _context.Users
                              .Include(u => u.Contacts)
                              .ThenInclude(u => u.Categories)
                              .Include(u => u.Categories)
                              .FirstOrDefault(u => u.Id == appUserId)!;

            var categories = appUser.Categories;

            if (categoryId == 0)
            {
                contacts = appUser.Contacts
                                  .OrderBy(c => c.LastName)
                                  .ThenBy(c => c.FirstName)
                                  .ToList();
            }
            else
            {
                contacts = appUser.Categories.FirstOrDefault(c => c.Id == categoryId)!
                                             .Contacts
                                             .OrderBy(c => c.LastName)
                                             .ThenBy(c => c.FirstName)
                                             .ToList();
            }

            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", categoryId);

            return View(contacts);

        }

        public IActionResult SearchContacts(string searchString)
        {
            string appUserId = _userManager.GetUserId(User);
            List<Contact> contacts = new List<Contact>();

            AppUser appUser = _context.Users
                                      .Include(c => c.Contacts)
                                      .ThenInclude(c => c.Categories)
                                      .FirstOrDefault(u => u.Id == appUserId);

            if (String.IsNullOrEmpty(searchString))
            {
                contacts = appUser.Contacts
                                   .OrderBy(c => c.LastName)
                                   .ThenBy(c => c.FirstName)
                                   .ToList();
            }
            else
            {
                contacts = appUser.Contacts.Where(c => c.FullName!.ToLower().Contains(searchString.ToLower()))
                                   .OrderBy(c => c.LastName)
                                   .ThenBy(c => c.FullName)
                                   .ToList();
            }

            ViewData["CategoryId"] = new SelectList(appUser.Categories, "Id", "Name", 0);

            return View(nameof(Index), contacts);
        }

        public async Task<IActionResult> EmailContact(int id)
        {
            string appUserId = _userManager?.GetUserId(User);
            Contact contact = await _context.Contacts.Where(c => c.Id == id && c.AppUserID == appUserId)
                                                     .FirstOrDefaultAsync();

            if (contact == null)
            {
                return NotFound();
            }

            EmailData emailData = new EmailData()
            {
                EmailAddress = contact.Email,
                FirstName = contact.FirstName,
                LastName = contact.LastName
            };

            EmailContactViewModel model = new EmailContactViewModel()
            {
                Contact = contact,
                EmailData = emailData
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EmailContact(EmailContactViewModel ecvm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _emailService.SendEmailAsync(ecvm.EmailData.EmailAddress, ecvm.EmailData.Subject, ecvm.EmailData.Body);
                    return RedirectToAction("Index", "Contacts", new { swalMessage = "Success: Email Sent!"});
                }
                catch 
                {
                    return RedirectToAction("Index", "Contacts", new { swalMessage = "Error: Email Send Faild!" });
                    throw;
                }

            }
            return View(ecvm);
        }

        // GET: Contacts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Contacts/Create
        public async Task<IActionResult> Create()
        {
            string appUserId = _userManager.GetUserId(User);

            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _addressBookService.GetUserCategoriesAsync(appUserId), "Id", "Name");

            return View();
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,ImageFile")] Contact contact, List<int> CategoryList)
        {
            ModelState.Remove("AppUserId");

            if (ModelState.IsValid)
            {
                contact.AppUserID = _userManager.GetUserId(User);
                contact.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

                if (contact.BirthDate != null)
                {
                    contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                }

                if (contact.ImageFile != null)
                {
                    contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                    contact.ImageType = contact.ImageFile.ContentType;
                }

                _context.Add(contact);
                await _context.SaveChangesAsync();

                // Loop over all the selected categories.
                foreach (int categoryId in CategoryList)
                {
                    await _addressBookService.AddContactToCategoryAsync(categoryId, contact.Id);
                }               

            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string appUserId = _userManager.GetUserId(User);

            //var contact = await _context.Contacts.FindAsync(id);
            var contact = await _context.Contacts.Where(c => c.Id == id && c.AppUserID == appUserId)
                                                 .FirstOrDefaultAsync();

            if (contact == null)
            {
                return NotFound();
            }

            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _addressBookService.GetUserCategoriesAsync(appUserId), "Id", "Name", await _addressBookService.GetContactCategoryIdsAsync(contact.Id));

            return View(contact);
        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserID,FirstName,LastName,BirthDate,Address1,Address2,City,state,ZipCode,Email,PhoneNumber,Created,ImageFile,ImageData,ImageType")] Contact contact, List<int> CategoryList)
        {
            if (id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    contact.Created = DateTime.SpecifyKind(contact.Created, DateTimeKind.Utc);

                    if(contact.BirthDate != null)
                    {
                        contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                    }

                    if (contact.ImageFile != null)
                    {
                        contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                        contact.ImageType = contact.ImageFile.ContentType;
                    }

                    _context.Update(contact);
                    await _context.SaveChangesAsync();

                    //Save the categories
                    // Remove the current categories
                    List<Category> oldCategories = (await _addressBookService.GetContactCategoriesAsync(contact.Id)).ToList();

                    foreach (var category in oldCategories)
                    {
                        await _addressBookService.RemoveContactFromCategoryAsync(category.Id, contact.Id);
                    }

                    //Add the selected categories
                    foreach (int categoryid in CategoryList)
                    {
                        await _addressBookService.AddContactToCategoryAsync(categoryid, contact.Id);
                    }

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
                return RedirectToAction("Index");
            }

            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", contact.AppUserID);
            return View(contact);
        }

        // GET: Contacts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string appUserId = _userManager.GetUserId(User);

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.Id == id && c.AppUserID == appUserId);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string appUserId = _userManager?.GetUserId(User);

            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id && c.AppUserID == appUserId);

            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");         
        }

        private bool ContactExists(int id)
        {
            return _context.Contacts.Any(e => e.Id == id);
        }
    }
}
