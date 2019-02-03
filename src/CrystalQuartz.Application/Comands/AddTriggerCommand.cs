﻿using System;
using CrystalQuartz.Application.Comands.Inputs;
using CrystalQuartz.Application.Comands.Outputs;
using CrystalQuartz.Core.Contracts;
using CrystalQuartz.Core.Domain.TriggerTypes;

namespace CrystalQuartz.Application.Comands
{
    using System.Collections.Generic;
    using System.Linq;
    using CrystalQuartz.Core.Domain.ObjectInput;

    public class AddTriggerCommand : AbstractSchedulerCommand<AddTriggerInput, AddTriggerOutput>
    {
        private readonly RegisteredInputType[] _registeredInputTypes;

        public AddTriggerCommand(Func<SchedulerHost> schedulerHostProvider, RegisteredInputType[] registeredInputTypes) : base(schedulerHostProvider)
        {
            _registeredInputTypes = registeredInputTypes;
        }

        protected override void InternalExecute(AddTriggerInput input, AddTriggerOutput output)
        {
            IDictionary<string, object> jobDataMap = null;

            if (input.JobDataMap != null)
            {
                jobDataMap = new Dictionary<string, object>();

                IDictionary<string, string> validationErrors = new Dictionary<string, string>();

                foreach (JobDataItem item in input.JobDataMap)
                {
                    RegisteredInputType inputType = _registeredInputTypes.FirstOrDefault(x => x.InputType.Code == item.InputTypeCode);
                    if (inputType == null)
                    {
                        /*
                         * We can only get here if client-side input type
                         * definitions are not in sync with server-side.
                         */
                        validationErrors[item.Key] = "Unknown input type: " + item.InputTypeCode;
                    }
                    else
                    {
                        try
                        {
                            var value = inputType.Converter == null
                                ? item.Value
                                : inputType.Converter.Convert(item.Value);

                            jobDataMap[item.Key] = value;
                        }
                        catch (Exception ex)
                        {
                            validationErrors[item.Key] = ex.Message;
                        }
                    }
                }

                if (validationErrors.Any())
                {
                    output.ValidationErrors = validationErrors;

                    return;
                }
            }

            SchedulerHost.Commander.TriggerJob(
                input.Job, 
                input.Group, 
                input.Name, 
                CreateTriggerType(input),
                jobDataMap);
        }

        private static TriggerType CreateTriggerType(AddTriggerInput input)
        {
            switch (input.TriggerType)
            {
                case "Simple":
                    return new SimpleTriggerType(input.RepeatForever ? -1 : input.RepeatCount, input.RepeatInterval, 0 /* todo */);
                case "Cron":
                    return new CronTriggerType(input.CronExpression);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}