import { FormEvent, useState, useMemo } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  Button,
  Paper,
  Stack,
  TextField,
  Typography,
  Box,
  Collapse,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  InputAdornment,
  IconButton,
  Tooltip,
  Snackbar,
  Alert,
  CircularProgress
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import CloseIcon from "@mui/icons-material/Close";
import SearchIcon from "@mui/icons-material/Search";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";
import ClearIcon from "@mui/icons-material/Clear";
import { apiClient } from "../api/apiClient";
import { WarehouseDto } from "../api/types";
import { useAuth } from "../auth/AuthContext";
import { DataTable } from "../components/DataTable";

export function WarehousesPage() {
  const { user } = useAuth();
  const isAdmin = user?.role === "Admin";
  const queryClient = useQueryClient();

  // Create form state
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [formName, setFormName] = useState("");

  // Search state
  const [search, setSearch] = useState("");

  // Edit states
  const [editTarget, setEditTarget] = useState<WarehouseDto | null>(null);
  const [editName, setEditName] = useState("");

  // Delete states
  const [deleteTarget, setDeleteTarget] = useState<WarehouseDto | null>(null);

  // Notifications
  const [snack, setSnack] = useState<{ open: boolean; message: string; severity: "success" | "error" | "warning" }>({
    open: false,
    message: "",
    severity: "success"
  });

  // ── Queries & Mutations ──────────────────────────────────────────────────

  const warehousesQuery = useQuery<WarehouseDto[]>({
    queryKey: ["warehouses-full"],
    queryFn: async () => (await apiClient.get<WarehouseDto[]>("/warehouses")).data
  });

  const createMutation = useMutation({
    mutationFn: async () => apiClient.post<WarehouseDto>("/warehouses", { name: formName }),
    onSuccess: () => {
      setFormName("");
      setIsFormOpen(false);
      queryClient.invalidateQueries({ queryKey: ["warehouses-full"] });
      queryClient.invalidateQueries({ queryKey: ["reference-data"] });
      setSnack({ open: true, message: "Depo başarıyla oluşturuldu.", severity: "success" });
    },
    onError: (err: any) => {
      const msg = err?.response?.data?.error ?? "Depo oluşturulurken bir hata oluştu.";
      setSnack({ open: true, message: msg, severity: "error" });
    }
  });

  const updateMutation = useMutation({
    mutationFn: async () => apiClient.put(`/warehouses/${editTarget!.id}`, { name: editName }),
    onSuccess: () => {
      setEditTarget(null);
      queryClient.invalidateQueries({ queryKey: ["warehouses-full"] });
      queryClient.invalidateQueries({ queryKey: ["reference-data"] });
      setSnack({ open: true, message: "Depo bilgileri güncellendi.", severity: "success" });
    },
    onError: (err: any) => {
      const msg = err?.response?.data?.error ?? "Depo güncellenirken bir hata oluştu.";
      setSnack({ open: true, message: msg, severity: "error" });
    }
  });

  const deleteMutation = useMutation({
    mutationFn: async () => apiClient.delete(`/warehouses/${deleteTarget!.id}`),
    onSuccess: () => {
      setDeleteTarget(null);
      queryClient.invalidateQueries({ queryKey: ["warehouses-full"] });
      queryClient.invalidateQueries({ queryKey: ["reference-data"] });
      setSnack({ open: true, message: "Depo başarıyla silindi.", severity: "success" });
    },
    onError: (err: any) => {
      const msg = err?.response?.data?.error ?? "Bu depoya bağlı stok hareketi veya kasa varsa silinemez.";
      setSnack({ open: true, message: msg, severity: "error" });
    }
  });

  // ── Search Filtering ──────────────────────────────────────────────────────

  const filteredWarehouses = useMemo(() => {
    const list = warehousesQuery.data ?? [];
    const term = search.trim().toLowerCase();
    if (!term) return list;
    return list.filter(w => 
      w.name.toLowerCase().includes(term) || 
      w.id.toLowerCase().includes(term)
    );
  }, [warehousesQuery.data, search]);

  // ── Handlers ──────────────────────────────────────────────────────────────

  function handleCreateSubmit(e: FormEvent) {
    e.preventDefault();
    if (!formName.trim()) return;
    createMutation.mutate();
  }

  function handleEditSubmit(e: FormEvent) {
    e.preventDefault();
    if (!editName.trim()) return;
    updateMutation.mutate();
  }

  function openEdit(w: WarehouseDto) {
    setEditTarget(w);
    setEditName(w.name);
  }

  // ── Render ────────────────────────────────────────────────────────────────

  if (warehousesQuery.isLoading) {
    return (
      <Box sx={{ display: "flex", justifyContent: "center", mt: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Stack spacing={3}>
      {/* Header */}
      <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
        <Box>
          <Typography variant="h5" fontWeight={800}>
            Depolar
          </Typography>
          <Typography color="text.secondary" variant="body2">
            Kayıtlı depoları ve depolama konumlarını görüntüleyin ve yönetin
          </Typography>
        </Box>
        {isAdmin && (
          <Button
            variant="contained"
            color={isFormOpen ? "secondary" : "primary"}
            startIcon={isFormOpen ? <CloseIcon /> : <AddIcon />}
            onClick={() => setIsFormOpen(!isFormOpen)}
          >
            {isFormOpen ? "Vazgeç" : "Depo Ekle"}
          </Button>
        )}
      </Box>

      {/* Expandable Create Form (Admin only) */}
      {isAdmin && (
        <Collapse in={isFormOpen}>
          <Paper component="form" onSubmit={handleCreateSubmit} sx={{ p: 3, borderRadius: 2 }}>
            <Typography variant="subtitle1" fontWeight={700} sx={{ mb: 2 }}>
              Yeni Depo Tanımla
            </Typography>
            <Box sx={{ display: "flex", gap: 2, alignItems: "flex-start", flexWrap: "wrap" }}>
              <TextField
                label="Depo Adı"
                value={formName}
                onChange={(e) => setFormName(e.target.value)}
                required
                size="small"
                sx={{ minWidth: 280 }}
              />
              <Button 
                type="submit" 
                variant="contained" 
                disabled={createMutation.isPending || !formName.trim()}
                sx={{ height: 40 }}
              >
                {createMutation.isPending ? "Kaydediliyor..." : "Depoyu Kaydet"}
              </Button>
            </Box>
          </Paper>
        </Collapse>
      )}

      {/* Search Bar */}
      <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
        <TextField
          placeholder="Depo adı veya ID ile ara…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          size="small"
          sx={{ maxWidth: 380, flex: 1 }}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon fontSize="small" sx={{ color: "text.disabled" }} />
              </InputAdornment>
            ),
            endAdornment: search ? (
              <InputAdornment position="end">
                <IconButton size="small" onClick={() => setSearch("")}>
                  <ClearIcon fontSize="small" />
                </IconButton>
              </InputAdornment>
            ) : null,
          }}
        />
        {search && (
          <Typography variant="body2" color="text.secondary">
            {filteredWarehouses.length} / {warehousesQuery.data?.length ?? 0} sonuç
          </Typography>
        )}
      </Box>

      {/* Data Table */}
      <DataTable
        columns={isAdmin ? ["Depo Adı", "Depo ID", "İşlemler"] : ["Depo Adı", "Depo ID"]}
        rows={filteredWarehouses.map((w) => {
          const cells = [
            <Typography variant="body2" fontWeight={700} color="primary.main">{w.name}</Typography>,
            <Typography variant="body2" color="text.secondary">{w.id}</Typography>
          ];
          if (isAdmin) {
            cells.push(
              <Box sx={{ display: "flex", gap: 0.5 }}>
                <Tooltip title="Düzenle">
                  <IconButton size="small" color="primary" onClick={() => openEdit(w)}>
                    <EditIcon fontSize="small" />
                  </IconButton>
                </Tooltip>
                <Tooltip title="Sil">
                  <IconButton size="small" color="error" onClick={() => setDeleteTarget(w)}>
                    <DeleteIcon fontSize="small" />
                  </IconButton>
                </Tooltip>
              </Box>
            );
          }
          return cells;
        })}
      />

      {/* Edit Dialog */}
      <Dialog
        open={!!editTarget}
        onClose={() => setEditTarget(null)}
        fullWidth
        maxWidth="xs"
        PaperProps={{ sx: { borderRadius: 3 } }}
      >
        <DialogTitle sx={{ fontWeight: 700 }}>Depoyu Düzenle</DialogTitle>
        <Box component="form" onSubmit={handleEditSubmit}>
          <DialogContent>
            <TextField
              label="Depo Adı"
              value={editName}
              onChange={(e) => setEditName(e.target.value)}
              required
              fullWidth
              autoFocus
              size="small"
              sx={{ mt: 1 }}
            />
          </DialogContent>
          <DialogActions sx={{ px: 3, pb: 2 }}>
            <Button variant="outlined" onClick={() => setEditTarget(null)}>
              İptal
            </Button>
            <Button type="submit" variant="contained" disabled={updateMutation.isPending || !editName.trim()}>
              {updateMutation.isPending ? "Kaydediliyor..." : "Değişiklikleri Kaydet"}
            </Button>
          </DialogActions>
        </Box>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        maxWidth="xs"
        fullWidth
        PaperProps={{ sx: { borderRadius: 3 } }}
      >
        <DialogTitle sx={{ fontWeight: 700 }}>Depoyu Sil</DialogTitle>
        <DialogContent>
          <Typography>
            <strong>{deleteTarget?.name}</strong> deposunu silmek istediğinizden emin misiniz?
            Bu işlem geri alınamaz.
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1.5 }}>
            Not: Bu depoya bağlı aktif stok hareketi veya kasalar varsa silme işlemi başarısız olacaktır.
          </Typography>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button variant="outlined" onClick={() => setDeleteTarget(null)}>
            İptal
          </Button>
          <Button
            variant="contained"
            color="error"
            disabled={deleteMutation.isPending}
            onClick={() => deleteMutation.mutate()}
          >
            {deleteMutation.isPending ? "Siliniyor..." : "Evet, Sil"}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Notification Snackbar */}
      <Snackbar
        open={snack.open}
        autoHideDuration={4000}
        onClose={() => setSnack(s => ({ ...s, open: false }))}
        anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
      >
        <Alert
          severity={snack.severity}
          variant="filled"
          onClose={() => setSnack(s => ({ ...s, open: false }))}
          sx={{ borderRadius: 2, fontWeight: 600 }}
        >
          {snack.message}
        </Alert>
      </Snackbar>
    </Stack>
  );
}
