import { useId } from "react";
import type { FormValues, ResourceField, ResourceOption } from "./types";

function resolveOptions(field: ResourceField, values: FormValues): ResourceOption[] {
  if (!field.options) return [];
  return typeof field.options === "function" ? field.options(values) : field.options;
}

interface ResourceFieldInputProps {
  field: ResourceField;
  values: FormValues;
  onChange: (field: ResourceField, value: string) => void;
}

export function ResourceFieldInput({ field, values, onChange }: ResourceFieldInputProps) {
  const helpId = useId();
  const inputId = useId();
  const options = resolveOptions(field, values);
  const value = values[field.name] ?? "";
  const describedBy = field.helpText ? helpId : undefined;
  const help = field.helpText ? <small id={helpId} className="field-hint">{field.helpText}</small> : null;

  if (field.type === "multiselect") {
    const selected = value.split(",").filter(Boolean);

    function toggle(optionValue: string, checked: boolean) {
      const next = checked
        ? [...selected, optionValue]
        : selected.filter((item) => item !== optionValue);
      onChange(field, Array.from(new Set(next)).join(","));
    }

    return (
      <fieldset className="checkbox-group" aria-describedby={describedBy}>
        <legend>
          {field.label}
          <span className="checkbox-count"> ({selected.length} seleccionados)</span>
        </legend>
        {options.length === 0 ? (
          <p className="field-hint">{field.placeholder ?? "No hay opciones disponibles."}</p>
        ) : (
          <div className="checkbox-grid">
            {options.map((option) => {
              const optionValue = String(option.value);
              return (
                <label key={optionValue} className="checkbox-option">
                  <input
                    type="checkbox"
                    name={field.name}
                    value={optionValue}
                    checked={selected.includes(optionValue)}
                    onChange={(event) => toggle(optionValue, event.target.checked)}
                  />
                  <span>{option.label}</span>
                </label>
              );
            })}
          </div>
        )}
        {help}
      </fieldset>
    );
  }

  return (
    <div className={`form-field${field.type === "textarea" ? " full-width" : ""}`}>
      <label htmlFor={inputId}>
        {field.label}
        {field.required && <span className="required-mark" aria-hidden="true"> *</span>}
      </label>
      {field.type === "textarea" ? (
        <textarea
          id={inputId}
          name={field.name}
          required={field.required}
          value={value}
          placeholder={field.placeholder}
          maxLength={field.maxLength}
          aria-describedby={describedBy}
          onChange={(event) => onChange(field, event.target.value)}
        />
      ) : field.type === "select" ? (
        <select
          id={inputId}
          name={field.name}
          required={field.required}
          value={value}
          aria-describedby={describedBy}
          onChange={(event) => onChange(field, event.target.value)}
        >
          <option value="">{field.placeholder ?? "Selecciona una opción"}</option>
          {options.map((option) => (
            <option key={String(option.value)} value={option.value}>{option.label}</option>
          ))}
        </select>
      ) : (
        <input
          id={inputId}
          name={field.name}
          type={field.type ?? "text"}
          required={field.required}
          min={field.min}
          max={field.max}
          minLength={field.minLength}
          maxLength={field.maxLength}
          autoComplete={field.autoComplete}
          placeholder={field.placeholder}
          value={value}
          aria-describedby={describedBy}
          onChange={(event) => onChange(field, event.target.value)}
        />
      )}
      {help}
    </div>
  );
}
