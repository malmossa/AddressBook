using AddressBook.Data;
using AddressBook.Models;
using AddressBook.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AddressBook.Services
{
    public class AddressBookService : IAddressBookService
    {
        private readonly ApplicationDbContext _context;

        public AddressBookService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddContactToCategoryAsync(int categoryId, int contactId)
        {
            try
            {
                // Check to see if the category is un the contact
                if (!await IsContactInCategory(categoryId, contactId))
                {
                    Contact? contact = await _context.Contacts.FindAsync(contactId);
                    Category? category = await _context.Categories.FindAsync(categoryId);

                    if (category != null && contact != null)
                    {
                        category.Contacts.Add(contact);
                        await _context.SaveChangesAsync();
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<ICollection<Category>> GetContactCategoriesAsync(int contactId)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<int>> GetContactCategoryIdsAsync(int contactId)
        {
           try
            {
                var contact = await _context.Contacts.Include(c => c.Categories)
                                                     .FirstOrDefaultAsync(c => c.Id == contactId); 

                List<int> categoryIds = contact.Categories.Select(c => c.Id).ToList();

                return categoryIds;

            } catch(Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Category>> GetUserCategoriesAsync(string userId)
        {
            List<Category> categories = new List<Category>();

            try
            {
                categories = await _context.Categories.Where(c => c.AppUserID == userId)
                                                      .OrderBy(c => c.Name)
                                                      .ToListAsync();
            } 
            catch
            {
                throw;
            }

            return categories;
        }

        public async Task<bool> IsContactInCategory(int categoryId, int contactId)
        {
            Contact? contact = await _context.Contacts.FindAsync(contactId);

            return await _context.Categories
                                 .Include(c => c.Contacts)
                                 .Where(c => c.Id == categoryId && c.Contacts.Contains(contact))
                                 .AnyAsync();
        }

        public Task RemoveContactFromCategoryAsync(int categoryId, int contactId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Contact> SearchForContacts(string searchString, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
