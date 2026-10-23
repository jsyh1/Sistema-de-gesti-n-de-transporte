import { useCallback, useEffect, useMemo, useState } from 'react'
import type { FormEvent, ReactNode } from 'react'
import { Icon } from './Icon'
import { mensajeError } from '../services/api'

export interface FormField {
  name: string
  label: string
  type?: 'text' | 'number' | 'password' | 'select' | 'checkbox'
  required?: boolean
  maxLength?: number
  minLength?: number
  min?: number
  placeholder?: string
  helpText?: string
  options?: { value: string; label: string }[]
}

export interface CrudDefinition<T extends { id: number }> {
  title: string
  singular: string
  description: string
  searchPlaceholder: string
  list: () => Promise<T[]>
  create: (form: Record<string, string>) => Promise<unknown>
  update: (id: number, form: Record<string, string>) => Promise<unknown>
  remove: (id: number) => Promise<void>
  toForm: (item?: T) => Record<string, string>
  fields: FormField[]
  columns: { title: string; render: (item: T) => ReactNode }[]
  searchableText: (item: T) => string
}

export function CrudPage<T extends { id: number }>({ definition }: { definition: CrudDefinition<T> }) {
  const [items, setItems] = useState<T[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [query, setQuery] = useState('')
  const [editing, setEditing] = useState<T | null>(null)
  const [modalOpen, setModalOpen] = useState(false)
  const [form, setForm] = useState<Record<string, string>>({})
  const [saving, setSaving] = useState(false)
  const [deletingId, setDeletingId] = useState<number | null>(null)

  const loadItems = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      setItems(await definition.list())
    } catch (cause) {
      setError(mensajeError(cause))
    } finally {
      setLoading(false)
    }
  }, [definition])

  useEffect(() => {
    let active = true
    definition.list().then((records) => {
      if (active) setItems(records)
    }).catch((cause: unknown) => {
      if (active) setError(mensajeError(cause))
    }).finally(() => {
      if (active) setLoading(false)
    })
    return () => { active = false }
  }, [definition])
  useEffect(() => {
    if (!notice) return
    const timeout = window.setTimeout(() => setNotice(''), 3600)
    return () => window.clearTimeout(timeout)
  }, [notice])

  const filteredItems = useMemo(() => {
    const normalizedQuery = query.trim().toLocaleLowerCase('es')
    return normalizedQuery
      ? items.filter((item) => definition.searchableText(item).toLocaleLowerCase('es').includes(normalizedQuery))
      : items
  }, [definition, items, query])

  const openCreate = () => {
    setEditing(null)
    setForm(definition.toForm())
    setModalOpen(true)
  }

  const openEdit = (item: T) => {
    setEditing(item)
    setForm(definition.toForm(item))
    setModalOpen(true)
  }

  const closeModal = () => {
    if (saving) return
    setModalOpen(false)
    setEditing(null)
  }

  const save = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setSaving(true)
    try {
      if (editing) await definition.update(editing.id, form)
      else await definition.create(form)
      setNotice(`${definition.singular.charAt(0).toUpperCase()}${definition.singular.slice(1)} ${editing ? 'actualizado' : 'creado'} correctamente.`)
      setModalOpen(false)
      await loadItems()
    } catch (cause) {
      setError(mensajeError(cause))
    } finally {
      setSaving(false)
    }
  }

  const remove = async (item: T) => {
    if (!window.confirm(`¿Eliminar este registro de ${definition.singular}? Esta acción no se puede deshacer.`)) return
    setDeletingId(item.id)
    setError('')
    try {
      await definition.remove(item.id)
      setItems((current) => current.filter((entry) => entry.id !== item.id))
      setNotice(`${definition.singular.charAt(0).toUpperCase()}${definition.singular.slice(1)} eliminado correctamente.`)
    } catch (cause) {
      setError(mensajeError(cause))
    } finally {
      setDeletingId(null)
    }
  }

  return (
    <>
      <section className="page-heading">
        <div><div className="eyebrow">ADMINISTRACIÓN</div><h1>{definition.title}</h1><p>{definition.description}</p></div>
        <button className="button button-primary" onClick={openCreate}><Icon name="plus" size={18} />Agregar {definition.singular}</button>
      </section>
      {notice && <div className="notice notice-success" role="status">{notice}<button onClick={() => setNotice('')} aria-label="Cerrar aviso"><Icon name="close" size={16} /></button></div>}
      {error && <div className="notice notice-error" role="alert"><span>{error}</span><button className="button button-quiet" onClick={() => void loadItems()}><Icon name="refresh" size={16} />Reintentar</button></div>}
      <section className="panel resource-panel">
        <div className="resource-toolbar">
          <div><h2>Registros</h2><p>{items.length} {items.length === 1 ? 'registro' : 'registros'} en total</p></div>
          <label className="search-field"><Icon name="search" size={18} /><input aria-label={definition.searchPlaceholder} placeholder={definition.searchPlaceholder} value={query} onChange={(event) => setQuery(event.target.value)} /></label>
        </div>
        {loading ? <div className="loading-state"><span className="spinner" />Cargando registros…</div>
          : filteredItems.length === 0 ? <div className="empty-state"><div className="empty-icon"><Icon name={items.length ? 'search' : 'plus'} size={22} /></div><h3>{items.length ? 'Sin resultados' : `Todavía no hay ${definition.title.toLocaleLowerCase('es')}`}</h3><p>{items.length ? 'Prueba con otro término de búsqueda.' : `Agrega el primer ${definition.singular} para comenzar.`}</p>{!items.length && <button className="button button-secondary" onClick={openCreate}><Icon name="plus" size={17} />Crear registro</button>}</div>
            : <div className="table-scroll"><table><thead><tr>{definition.columns.map((column) => <th key={column.title}>{column.title}</th>)}<th className="action-column">Acciones</th></tr></thead><tbody>{filteredItems.map((item) => <tr key={item.id}>{definition.columns.map((column) => <td key={column.title}>{column.render(item)}</td>)}<td><div className="row-actions"><button className="icon-button action-edit" aria-label={`Editar ${definition.singular}`} title="Editar" onClick={() => openEdit(item)}><Icon name="edit" size={17} /></button><button className="icon-button action-delete" aria-label={`Eliminar ${definition.singular}`} title="Eliminar" disabled={deletingId === item.id} onClick={() => void remove(item)}><Icon name="trash" size={17} /></button></div></td></tr>)}</tbody></table></div>}
      </section>
      <p className="resource-note">Los cambios se guardan directamente en el sistema de transporte.</p>
      {modalOpen && <div className="modal-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget) closeModal() }}>
        <section className="modal-card" role="dialog" aria-modal="true" aria-labelledby="form-title">
          <header className="modal-header"><div><span className="eyebrow">{editing ? 'ACTUALIZAR REGISTRO' : 'NUEVO REGISTRO'}</span><h2 id="form-title">{editing ? `Editar ${definition.singular}` : `Agregar ${definition.singular}`}</h2></div><button className="icon-button" aria-label="Cerrar" onClick={closeModal}><Icon name="close" /></button></header>
          <form onSubmit={(event) => void save(event)}>
            {definition.fields.map((field) => <label className={`form-field ${field.type === 'checkbox' ? 'form-checkbox' : ''}`} key={field.name}>
              {field.type === 'checkbox'
                ? <><input type="checkbox" checked={form[field.name] === 'true'} onChange={(event) => setForm((current) => ({ ...current, [field.name]: String(event.target.checked) }))} /><span>{field.label}</span></>
                : <><span>{field.label}{field.required && !(editing && field.type === 'password') && <b aria-hidden="true"> *</b>}</span>
                  {field.type === 'select'
                    ? <select required={field.required} value={form[field.name] ?? ''} onChange={(event) => setForm((current) => ({ ...current, [field.name]: event.target.value }))}>{field.options?.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}</select>
                    : <input type={field.type ?? 'text'} required={field.required && !(editing && field.type === 'password')} min={field.min} minLength={editing && field.type === 'password' ? undefined : field.minLength} maxLength={field.maxLength} placeholder={field.placeholder} value={form[field.name] ?? ''} onChange={(event) => setForm((current) => ({ ...current, [field.name]: event.target.value }))} />}
                  {field.helpText && <small>{field.helpText}</small>}</>}
            </label>)}
            <div className="modal-actions"><button className="button button-secondary" type="button" disabled={saving} onClick={closeModal}>Cancelar</button><button className="button button-primary" type="submit" disabled={saving}>{saving && <span className="spinner spinner-light" />}{saving ? 'Guardando…' : editing ? 'Guardar cambios' : 'Crear registro'}</button></div>
          </form>
        </section>
      </div>}
    </>
  )
}
