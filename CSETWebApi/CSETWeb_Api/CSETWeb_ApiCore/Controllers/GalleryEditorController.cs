//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Business.GalleryParser;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CSETWebCore.Api.Controllers
{   
    /// <summary>
    /// Provides endpoints for gallery configuration management in CSET.
    /// This controller handles the creation, modification, and deletion of gallery items and groups,
    /// allowing administrators to customize the assessment interface layout and available tools.
    /// Note: This controller is marked as obsolete and may be removed in future versions.
    /// </summary>
    [Obsolete("No longer in use")]
    [ApiController]
    public class GalleryEditorController : ControllerBase
    {

        private readonly ITokenManager _token;
        private CSETContext _context;
        private IGalleryEditor _galleryEditor;

        // if you want to use the gallery editor, change this to true
        private bool inDev = true;

        /// <summary>
        /// Initializes a new instance of the GalleryEditorController.
        /// </summary>
        /// <param name="token">Token manager for authentication and authorization</param>
        /// <param name="galleryEditor">Gallery editor business logic service</param>
        /// <param name="context">Database context for gallery operations</param>
        public GalleryEditorController(ITokenManager token, IGalleryEditor galleryEditor, CSETContext context)
        {
            _token = token;
            _context = context;
            _galleryEditor = galleryEditor;
        }

        /// <summary>
        /// Updates the position of a gallery item within its group.
        /// </summary>
        /// <param name="moveItem">Object containing the item to move and its new position</param>
        /// <returns>
        /// 200 OK if position updated successfully
        /// 400 Bad Request if the operation fails
        /// </returns>
        /// <remarks>
        /// This endpoint allows reordering of gallery items within their respective groups.
        /// The moveItem parameter should contain the item identifier and target position information.
        /// This functionality is currently disabled in production (inDev = false).
        /// </remarks>
        [HttpPost]
        [Route("api/galleryEdit/updatePosition")]
        public IActionResult updatePosition(MoveItem moveItem)
        {
            if (!inDev)
            {
                return Ok(200);
            }
            try
            {
                _galleryEditor.UpdatePosition(moveItem);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Clones a gallery item to create a duplicate with a new identifier.
        /// </summary>
        /// <param name="Item_To_Clone">GUID string of the item to clone</param>
        /// <param name="Group_Id">ID of the target group for the cloned item</param>
        /// <param name="New_Id">Whether to generate a new ID for the cloned item</param>
        /// <returns>
        /// 200 OK if item cloned successfully
        /// 400 Bad Request if the operation fails
        /// </returns>
        /// <remarks>
        /// Creates an exact copy of the specified gallery item in the target group.
        /// The cloned item will have the same properties as the original unless New_Id is true.
        /// This functionality is currently disabled in production (inDev = false).
        /// </remarks>
        [HttpGet]
        [Route("api/gallery/cloneItem")]
        public IActionResult CloneItem(String Item_To_Clone, int Group_Id, bool New_Id)
        {
            if (!inDev)
            {
                return Ok(200);
            }
            try
            {
                Guid Item_To_Clone_Guid = Guid.Parse(Item_To_Clone);
                _galleryEditor.CloneGalleryItem(Item_To_Clone_Guid, Group_Id, New_Id);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Clones an entire gallery group to create a duplicate group structure.
        /// </summary>
        /// <param name="Group_Id">ID of the group to clone</param>
        /// <param name="layout_Name">Name of the layout for the new group</param>
        /// <returns>
        /// 200 OK if group cloned successfully
        /// 400 Bad Request if the operation fails
        /// </returns>
        /// <remarks>
        /// Creates a copy of the specified gallery group including all its items and structure.
        /// The new group will be created with the specified layout name.
        /// This functionality is currently disabled in production (inDev = false).
        /// </remarks>
        [HttpGet]
        [Route("api/gallery/cloneGroup")]
        public IActionResult CloneGroup(int Group_Id, string layout_Name)
        {
            if (!inDev)
            {
                return Ok(200);
            }
            try
            {
                _galleryEditor.CloneGalleryGroup(Group_Id, layout_Name);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Adds a new gallery item to a specified group.
        /// </summary>
        /// <param name="newDescription">Description text for the new item</param>
        /// <param name="newTitle">Title text for the new item</param>
        /// <param name="newIconSmall">Filename for the small icon</param>
        /// <param name="newIconLarge">Filename for the large icon</param>
        /// <param name="newConfigSetup">Configuration setup string for the item</param>
        /// <param name="group_Id">ID of the group to add the item to</param>
        /// <param name="columnId">Column ID where the item should be placed</param>
        /// <returns>
        /// 200 OK if item added successfully
        /// 400 Bad Request if the operation fails
        /// </returns>
        /// <remarks>
        /// Creates a new gallery item with the specified properties and adds it to the target group.
        /// The item will be positioned in the specified column within the group.
        /// This functionality is currently disabled in production (inDev = false).
        /// </remarks>
        [HttpGet]
        [Route("api/gallery/addItem")]
        public IActionResult AddItem(string newDescription, string newTitle, string newIconSmall, string newIconLarge, string newConfigSetup, int group_Id, int columnId)
        {
            if (!inDev)
            {
                return Ok(200);
            }

            try
            {
                _galleryEditor.AddGalleryItem(newIconSmall, newIconLarge, newDescription, newTitle, newConfigSetup, group_Id, columnId);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Adds a new gallery group and an initial item to that group.
        /// </summary>
        /// <param name="group">Name of the new group</param>
        /// <param name="layout">Layout name for the group</param>
        /// <param name="newDescription">Description for the initial item</param>
        /// <param name="newTitle">Title for the initial item</param>
        /// <param name="newIconSmall">Small icon filename for the initial item</param>
        /// <param name="newIconLarge">Large icon filename for the initial item</param>
        /// <param name="newConfigSetup">Configuration setup for the initial item</param>
        /// <param name="columnId">Column ID for the initial item</param>
        /// <returns>
        /// 200 OK if group and item added successfully
        /// 400 Bad Request if the operation fails
        /// </returns>
        /// <remarks>
        /// Creates a new gallery group and immediately adds an initial item to it.
        /// This is a convenience method for creating groups with content in one operation.
        /// This functionality is currently disabled in production (inDev = false).
        /// </remarks>
        [HttpGet]
        [Route("api/gallery/addGroup")]
        public IActionResult AddGroup(string group, string layout, string newDescription, string newTitle, string newIconSmall, string newIconLarge, string newConfigSetup, int columnId)
        {
            if (!inDev)
            {
                return Ok(200);
            }

            try
            {
                var group_id = _galleryEditor.AddGalleryGroup(group, layout);
                _galleryEditor.AddGalleryItem(newIconSmall, newIconLarge, newDescription, newTitle, newConfigSetup, group_id, columnId);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Deletes a gallery item from a group.
        /// </summary>
        /// <param name="galleryItemGuid">GUID string of the item to delete</param>
        /// <param name="group_id">ID of the group containing the item</param>
        /// <returns>
        /// 200 OK if item deleted successfully
        /// 400 Bad Request if the operation fails
        /// </returns>
        /// <remarks>
        /// Removes the specified gallery item from its group.
        /// The item is permanently deleted and cannot be recovered.
        /// This functionality is currently disabled in production (inDev = false).
        /// </remarks>
        [HttpGet]
        [Route("api/gallery/deleteGalleryItem")]
        public IActionResult DeleteItem(string galleryItemGuid, int group_id)
        {
            if (!inDev)
            {
                return Ok(200);
            }
            try
            {
                Guid guid = Guid.Parse(galleryItemGuid);
                _galleryEditor.DeleteGalleryItem(guid, group_id);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Retrieves all available gallery layouts.
        /// </summary>
        /// <returns>
        /// 200 OK with list of layouts if successful
        /// 400 Bad Request if the operation fails
        /// </returns>
        /// <remarks>
        /// Returns a collection of all available gallery layout configurations.
        /// Layouts define the structure and appearance of gallery groups.
        /// This functionality is currently disabled in production (inDev = false).
        /// </remarks>
        [HttpGet]
        [Route("api/gallery/getlayouts")]
        public IActionResult GetLayouts()
        {
            if (!inDev)
            {
                return Ok(200);
            }
            try
            {

                return Ok(_galleryEditor.GetLayouts());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Deletes a gallery group and all its items.
        /// </summary>
        /// <param name="id">ID of the group to delete</param>
        /// <param name="layout">Layout name associated with the group</param>
        /// <returns>
        /// 200 OK if group deleted successfully
        /// 400 Bad Request if the operation fails
        /// </returns>
        /// <remarks>
        /// Removes the specified gallery group and all items contained within it.
        /// This operation is permanent and cannot be undone.
        /// This functionality is currently disabled in production (inDev = false).
        /// </remarks>
        [HttpGet]
        [Route("api/gallery/deleteGalleryGroup")]
        public IActionResult DeleteGroup(int id, string layout)
        {
            if (!inDev)
            {
                return Ok(200);
            }
            try
            {
                _galleryEditor.DeleteGalleryGroup(id, layout);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Updates the name/title of a gallery item or group.
        /// </summary>
        /// <param name="item">Object containing the item to update and its new value</param>
        /// <returns>
        /// 200 OK if name updated successfully
        /// 400 Bad Request if the operation fails or item not found
        /// </returns>
        /// <remarks>
        /// Updates the title of either a gallery item or group based on the IsGroup property.
        /// For groups, updates the Group_Title; for items, updates the Title property.
        /// This functionality is currently disabled in production (inDev = false).
        /// </remarks>
        [HttpPost]
        [Route("api/galleryEdit/updateName")]
        public IActionResult UpdateName([FromBody] UpdateItem item)
        {
            if (!inDev)
            {
                return Ok(200);
            }
            try
            {
                if (item.IsGroup)
                {
                    var galleryGroup = _context.GALLERY_GROUP.Where(x => x.Group_Id == item.Group_Id).FirstOrDefault();
                    if (galleryGroup == null) return BadRequest();

                    galleryGroup.Group_Title = item.Value;
                    _context.SaveChanges();
                }
                else
                {
                    Guid guid = Guid.Parse(item.Gallery_Item_Guid);
                    var galleryItem = _context.GALLERY_ITEM.Where(x => x.Gallery_Item_Guid == guid).FirstOrDefault();
                    if (galleryItem == null) return BadRequest();

                    galleryItem.Title = item.Value;
                    _context.SaveChanges();
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Updates the properties of a gallery item.
        /// </summary>
        /// <param name="item">Gallery item object with updated properties</param>
        /// <returns>
        /// 200 OK if item updated successfully
        /// 400 Bad Request if the operation fails or item not found
        /// </returns>
        /// <remarks>
        /// Updates all properties of the specified gallery item including title, description,
        /// configuration setup, icon filenames, and visibility status.
        /// This functionality is currently disabled in production (inDev = false).
        /// </remarks>
        [HttpPost]
        [Route("api/galleryEdit/updateItem")]
        public IActionResult UpdateItem([FromBody] GALLERY_ITEM item)
        {
            if (!inDev)
            {
                return Ok(200);
            }
            try
            {

                var galleryItem = _context.GALLERY_ITEM.Where(x => x.Gallery_Item_Guid == item.Gallery_Item_Guid).FirstOrDefault();
                if (galleryItem == null) return BadRequest();

                galleryItem.Title = item.Title;
                galleryItem.Description = item.Description;
                galleryItem.Configuration_Setup = item.Configuration_Setup;
                galleryItem.Icon_File_Name_Large = item.Icon_File_Name_Large;
                galleryItem.Icon_File_Name_Small = item.Icon_File_Name_Small;
                galleryItem.Is_Visible = item.Is_Visible;

                _context.SaveChanges();


                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Retrieves unused gallery items for a specific layout.
        /// </summary>
        /// <param name="Layout_Name">Name of the layout to check for unused items</param>
        /// <returns>
        /// 200 OK with list of unused items if successful
        /// 400 Bad Request if the operation fails
        /// </returns>
        /// <remarks>
        /// Returns gallery items that are not currently being used in the specified layout.
        /// These items can be added to the layout or repurposed as needed.
        /// This functionality is currently disabled in production (inDev = false).
        /// </remarks>
        [HttpGet]
        [Route("api/gallery/getUnused")]
        public IActionResult GetUnusedItems(string Layout_Name)
        {
            if (!inDev)
            {
                return Ok(200);
            }
            try
            {
                return Ok(_galleryEditor.GetUnused(Layout_Name));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


    }

}
