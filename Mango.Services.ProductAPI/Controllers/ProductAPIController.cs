using AutoMapper;
using Azure;
using Mango.Services.ProductAPI.Data;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dto;
using Mango.Services.ProductAPI.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Cryptography.Xml;

namespace Mango.Services.CouponAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductAPIController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly ResponsDTO _responsDTO;
        private readonly IMapper _mapper;

        public ProductAPIController(AppDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _responsDTO = new ResponsDTO();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ResponsDTO Get()
        {
            try
            {
                IEnumerable<Product> coupons = _context.Products.ToList();
                _responsDTO.Result = _mapper.Map<IEnumerable<ProductDto>>(coupons);
            }
            catch (Exception ex)
            {
                _responsDTO.IsSuccess = false;
                _responsDTO.Message = ex.Message;
            }

            return _responsDTO;
        }


        [HttpGet("{id}")]
        public ResponsDTO Get(int id)
        {
            try
            {
                Product coupons = _context.Products.First(el => el.ProductId == id);
                _responsDTO.Result = _mapper.Map<ProductDto>(coupons);
            }
            catch (Exception ex)
            {
                _responsDTO.IsSuccess = false;
                _responsDTO.Message = ex.Message;
            }

            return _responsDTO;
        }


        [HttpPost]
        public ResponsDTO Post([FromBody] ProductDto couponDto)
        {
            try
            {
                Product obj = _mapper.Map<Product>(couponDto);
                _context.Add(obj);
                _context.SaveChanges();
                _responsDTO.Result = _mapper.Map<ProductDto>(obj);
            }
            catch (Exception ex)
            {

                _responsDTO.IsSuccess = false;
                _responsDTO.Message = ex.Message;
            }
            return _responsDTO;
        }

        [HttpPut]
        public ResponsDTO Put([FromBody] ProductDto couponDto)
        {
            try
            {
                Product obj = _mapper.Map<Product>(couponDto);
                _context.Products.Update(obj);
                _context.SaveChanges();

                _responsDTO.Result = _mapper.Map<ProductDto>(obj);
            }
            catch (Exception ex)
            {
                _responsDTO.IsSuccess = false;
                _responsDTO.Message = ex.Message;
            }
            return _responsDTO;
        }


        [HttpDelete]
        [Route("{id:int}")]
        public ResponsDTO Delete(int id)
        {
            try
            {
                Product obj = _context.Products.First(u => u.ProductId == id);
                _context.Products.Remove(obj);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _responsDTO.IsSuccess = false;
                _responsDTO.Message = ex.Message;
            }
            return _responsDTO;
        }
    }
}
