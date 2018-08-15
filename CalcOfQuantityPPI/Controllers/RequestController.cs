﻿using CalcOfQuantityPPI.Data;
using CalcOfQuantityPPI.Models;
using CalcOfQuantityPPI.ViewModels.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CalcOfQuantityPPI.Controllers
{
    public class RequestController : Controller
    {
        private ApplicationContext _context = new ApplicationContext();

        [HttpGet]
        public ActionResult CreateRequest()
        {
            SelectList parentDepartments =
                new SelectList(_context.Departments.Where(p => p.ParentDepartmentId == null), "Id", "Name");
            ViewBag.ParentDepartmentList = parentDepartments;
            SelectList subsidiaryDepartments =
                new SelectList(_context.Departments.Where(c => c.ParentDepartmentId == _context.Departments.FirstOrDefault(p => p.ParentDepartmentId == null).Id), "Id", "Name");
            ViewBag.SubsidiaryDepartmentList = subsidiaryDepartments;
            return View(new RequestViewModel());
        }

        [HttpPost]
        public string CreateRequest(RequestViewModel model)
        {
            Department department = _context.Departments.Find(model.DepartmentId);
            string s = "<b>Подразделение:</b> " + _context.Departments.FirstOrDefault(d => d.Id == department.ParentDepartmentId).Name + "<br />"
                + "<b>Дочернее подразделение:</b> " + _context.Departments.Find(model.DepartmentId).Name + "<br /><br />";
            for (int k = 0; k < model.ProfessionsTableViewModel.ProfessionId.Count(); k++)
            {
                //int? professionId = model.ProfessionsTableViewModel.Professions.Select(p => p.Id).ElementAt(k);
                int? professionId = model.ProfessionsTableViewModel.ProfessionId.ElementAt(k);
                s += professionId.Value;
                s += "<b>Профессия: " + _context.Professions.Find(professionId).Name + "</b> " +
                    "<i>(Численность: " + model.ProfessionsTableViewModel.QuantityOfEmployees.ElementAt(k) + ")</i><br />";
                try
                {
                    List<int?> ppiIdList = _context.PPIForProfession.Where(p => p.ProfessionId == professionId).Select(p => p.PPIId).ToList();
                    List<int?> ppiIdList2 = _context.PPIForProfession/*.Where(p => p.ProfessionId == professionId)*/.Select(p => p.PPIId).ToList();
                    for (int i = 0; i < ppiIdList.Count(); i++)
                    {
                        s += ppiIdList.ElementAt(i);
                        s += "<i>" + _context.PersonalProtectiveItems.Find(ppiIdList.ElementAt(i)).Name + "</i>: ";
                        s += "<b>" + model.ProfessionsTableViewModel.QuantityOfPPIForOneEmployee.ElementAt(professionId.Value) + "</b>";
                        s += " Всего: <i>" + model.ProfessionsTableViewModel.TotalQuantityOfPPI.ElementAt(k + i) + "</i><br />";
                    };
                }
                catch (Exception ex) { s += "Exception" + ex.Message + "<br />"; }
                s += "<br />";
            }
            return s;
        }

        #region Partial Views

        [HttpGet]
        public PartialViewResult SubsidiaryDepartmentList(int id)
        {
            return PartialView(_context.Departments.Where(c => c.ParentDepartmentId == id).ToList());
        }

        [HttpGet]
        public PartialViewResult ProfessionsAndPPITable(int id)
        {
            RequestViewModel model = new RequestViewModel
            { 
                ProfessionsTableViewModel = new ProfessionsTableViewModel
                {
                    Professions = GetProfessionsByDepartmentId(id),
                    PPIForProfession = GetPPIForProfession(),
                    PPI = GetPersonalProtectiveItems()
                }
            };
            return PartialView(model);
        }

        #endregion

        #region Helpers

        private List<Profession> GetProfessionsByDepartmentId(int id)
        {
            List<Profession> professions = new List<Profession>();
            Department department = _context.Departments.Find(id);
            if (isParentDepartment(department))
            {
                int? subsidiaryDepartmentId = GetFirstSubsidiaryDepartamentIdByParentDepartmentId(id);
                professions = GetProfessionsById(subsidiaryDepartmentId);
            }
            else
            {
                professions = GetProfessionsById(id);
            }
            return professions;
        }

        private bool isParentDepartment(Department department)
        {
            return department.ParentDepartmentId == null;
        }

        private List<Profession> GetProfessionsById(int? id)
        {
            List<Profession> professions = new List<Profession>();
            foreach (ProfessionsInDepartment profession in _context.ProfessionsInDepartment.Where(c => c.DepartmentId == id).ToList())
            {
                professions.Add(_context.Professions.Find(profession.ProfessionId));
            }
            return professions;
        }

        private int? GetFirstSubsidiaryDepartamentIdByParentDepartmentId(int id)
        {
            return _context.Departments.FirstOrDefault(d => d.ParentDepartmentId == id).Id;
        }

        private List<PPIForProfession> GetPPIForProfession()
        {
            return _context.PPIForProfession.ToList();
        }

        private List<PersonalProtectiveItem> GetPersonalProtectiveItems()
        {
            return _context.PersonalProtectiveItems.ToList();
        }

        #endregion
    }
}