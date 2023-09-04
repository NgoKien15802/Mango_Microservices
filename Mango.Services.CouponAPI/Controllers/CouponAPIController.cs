using AutoMapper;
using Azure;
using Mango.Services.CouponAPI.Data;
using Mango.Services.CouponAPI.Models;
using Mango.Services.CouponAPI.Models.DTO;
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
    public class CouponAPIController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ResponsDTO _responsDTO;
        private readonly IMapper _mapper;

        public CouponAPIController(AppDbContext context, IMapper mapper)
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
                IEnumerable<Coupon> coupons = _context.Coupons.ToList();
                _responsDTO.Result = _mapper.Map<IEnumerable<CouponDTO>>(coupons);
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
                Coupon coupons = _context.Coupons.First(el => el.CouponId == id);
                _responsDTO.Result = _mapper.Map<CouponDTO>(coupons);
            }
            catch (Exception ex)
            {
                _responsDTO.IsSuccess = false;
                _responsDTO.Message = ex.Message;
            }

            return _responsDTO;
        }


        [HttpGet("GetByCode/{code}")]
        public ResponsDTO GetByCode(string code)
        {
            try
            {
                Coupon obj = _context.Coupons.First(u => u.CouponCode.ToLower() == code.ToLower());
                _responsDTO.Result = _mapper.Map<CouponDTO>(obj);
            }
            catch (Exception ex)
            {
                _responsDTO.IsSuccess = false;
                _responsDTO.Message = ex.Message;
            }
            return _responsDTO;
        }

        [HttpPost]
        public ResponsDTO Post([FromBody] CouponDTO couponDto)
        {
            try
            {
                Coupon obj = _mapper.Map<Coupon>(couponDto);
                _context.Add(obj);
                _context.SaveChanges();
                _responsDTO.Result = _mapper.Map<CouponDTO>(obj);
            }
            catch (Exception ex)
            {

                _responsDTO.IsSuccess = false;
                _responsDTO.Message = ex.Message;
            }
            return _responsDTO;
        }

        [HttpPut]
        public ResponsDTO Put([FromBody] CouponDTO couponDto)
        {
            try
            {
                Coupon obj = _mapper.Map<Coupon>(couponDto);
                _context.Coupons.Update(obj);
                _context.SaveChanges();

                _responsDTO.Result = _mapper.Map<CouponDTO>(obj);
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
                Coupon obj = _context.Coupons.First(u => u.CouponId == id);
                _context.Coupons.Remove(obj);
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
